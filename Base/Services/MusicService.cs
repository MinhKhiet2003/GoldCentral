using Base.Data;
using Base.Entities;
using Base.Exceptions;
using Base.Models.Requests;
using Base.Models.Responses;
using Base.Services.IServices;
using Microsoft.EntityFrameworkCore;
using System.Buffers.Binary;

namespace Base.Services;

public class MusicService(AppDbContext db, IWebHostEnvironment environment, TimeProvider clock) : IMusicService
{
    public const long MaxAudioSize = 40 * 1024 * 1024;
    private static readonly HashSet<string> Departments = ["all", "vaccination", "dermatology", "rehabilitation", "wellness", "office"];

    public async Task<List<MusicTrackResponse>> ListAsync(string? department, bool includeUnpublished, CancellationToken cancellationToken)
    {
        var selectedDepartment = string.IsNullOrWhiteSpace(department) ? "all" : department.Trim().ToLowerInvariant();
        if (!Departments.Contains(selectedDepartment))
        {
            throw new ApiException(400, "Phòng ban được chọn không hợp lệ.");
        }

        var query = db.MusicTracks.AsNoTracking();
        if (!includeUnpublished)
        {
            query = query.Where(track => track.IsPublished);
        }

        if (selectedDepartment != "all")
        {
            query = query.Where(track => track.Department == selectedDepartment || track.Department == "all");
        }

        var tracks = await query.OrderBy(track => track.SortOrder).ThenBy(track => track.Title)
            .ToListAsync(cancellationToken);
        return tracks.Select(Map).ToList();
    }

    public async Task<MusicTrackResponse> SaveAsync(Guid? id, MusicTrackRequest request, CancellationToken cancellationToken)
    {
        var title = request.Title.Trim();
        var artist = string.IsNullOrWhiteSpace(request.Artist) ? null : request.Artist.Trim();
        var department = request.Department.Trim().ToLowerInvariant();
        if (title.Length == 0 || title.Length > 160)
        {
            throw new ApiException(400, "Tên bài hát phải có từ 1 đến 160 ký tự.");
        }

        if (artist?.Length > 120)
        {
            throw new ApiException(400, "Tên nghệ sĩ không được vượt quá 120 ký tự.");
        }

        if (!Departments.Contains(department))
        {
            throw new ApiException(400, "Phòng ban được chọn không hợp lệ.");
        }

        var track = id is null
            ? new MusicTrack { CreatedAt = clock.GetUtcNow().UtcDateTime }
            : await db.MusicTracks.SingleOrDefaultAsync(item => item.Id == id.Value, cancellationToken)
                ?? throw new ApiException(404, "Không tìm thấy bài hát.");

        if (track.Id == Guid.Empty) track.Id = Guid.NewGuid();
        track.Title = title;
        track.Artist = artist;
        track.Department = department;
        track.SortOrder = request.SortOrder;
        track.IsPublished = request.IsPublished;

        string? newPath = null;
        var previousStoredName = track.StoredFileName;
        try
        {
            if (request.File is not null)
            {
                var uploaded = await StoreAudioAsync(request.File, cancellationToken);
                newPath = uploaded.Path;
                track.OriginalFileName = uploaded.OriginalName;
                track.StoredFileName = uploaded.StoredName;
                track.ContentType = uploaded.ContentType;
                track.Size = uploaded.Size;
            }
            else if (id is null)
            {
                throw new ApiException(400, "Vui lòng chọn file nhạc MP3 hoặc WAV.");
            }

            if (id is null) db.MusicTracks.Add(track);
            await db.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            if (newPath is not null && File.Exists(newPath)) File.Delete(newPath);
            throw;
        }

        if (request.File is not null && !string.IsNullOrEmpty(previousStoredName))
        {
            DeleteAudio(previousStoredName);
        }

        return Map(track);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var track = await db.MusicTracks.SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (track is null) return false;

        db.MusicTracks.Remove(track);
        await db.SaveChangesAsync(cancellationToken);
        DeleteAudio(track.StoredFileName);
        return true;
    }

    public async Task<(string Path, string ContentType, string Title)?> GetStreamAsync(Guid id, CancellationToken cancellationToken)
    {
        var track = await db.MusicTracks.AsNoTracking()
            .Where(item => item.Id == id && item.IsPublished)
            .Select(item => new { item.StoredFileName, item.ContentType, item.Title })
            .SingleOrDefaultAsync(cancellationToken);
        if (track is null) return null;

        var path = AudioPath(track.StoredFileName);
        return File.Exists(path) ? (path, track.ContentType, track.Title) : null;
    }

    private async Task<(string Path, string OriginalName, string StoredName, string ContentType, long Size)> StoreAudioAsync(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file.Length == 0 || file.Length > MaxAudioSize)
        {
            throw new ApiException(400, "File nhạc phải có dữ liệu và không vượt quá 40 MB.");
        }

        var originalName = Path.GetFileName(file.FileName.Replace('\\', '/'));
        if (originalName.Length > 255) throw new ApiException(400, "Tên file không được vượt quá 255 ký tự.");

        var extension = Path.GetExtension(originalName).ToLowerInvariant();
        var contentType = extension switch
        {
            ".mp3" => "audio/mpeg",
            ".wav" => "audio/wav",
            _ => throw new ApiException(400, "Chỉ hỗ trợ file nhạc MP3 và WAV.")
        };

        await using var input = file.OpenReadStream();
        var header = new byte[12];
        var headerLength = await input.ReadAsync(header.AsMemory(), cancellationToken);
        var isValid = extension switch
        {
            ".mp3" => headerLength >= 3 && (header.AsSpan(0, 3).SequenceEqual("ID3"u8)
                || (header[0] == 0xFF && (header[1] & 0xE0) == 0xE0)),
            ".wav" => headerLength == 12 && header.AsSpan(0, 4).SequenceEqual("RIFF"u8)
                && header.AsSpan(8, 4).SequenceEqual("WAVE"u8),
            _ => false
        };

        if (!isValid) throw new ApiException(400, "Nội dung file không khớp với định dạng nhạc đã chọn.");

        var storedName = Guid.NewGuid().ToString("N") + extension;
        var directory = Path.Combine(environment.WebRootPath, "uploads", "music");
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, storedName);
        try
        {
            await using var output = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, true);
            await output.WriteAsync(header.AsMemory(0, headerLength), cancellationToken);
            var buffer = new byte[81920];
            long total = headerLength;
            int count;
            while ((count = await input.ReadAsync(buffer, cancellationToken)) > 0)
            {
                total += count;
                if (total > MaxAudioSize) throw new ApiException(400, "File nhạc không được vượt quá 40 MB.");
                await output.WriteAsync(buffer.AsMemory(0, count), cancellationToken);
            }

            return (path, originalName, storedName, contentType, total);
        }
        catch
        {
            if (File.Exists(path)) File.Delete(path);
            throw;
        }
    }

    private string AudioPath(string storedName) => Path.Combine(environment.WebRootPath, "uploads", "music", storedName);

    private void DeleteAudio(string storedName)
    {
        if (string.IsNullOrWhiteSpace(storedName)) return;
        var path = AudioPath(storedName);
        if (File.Exists(path)) File.Delete(path);
    }

    private MusicTrackResponse Map(MusicTrack track) => new()
    {
        Id = track.Id,
        Title = track.Title,
        Artist = track.Artist,
        Department = track.Department,
        IsPublished = track.IsPublished,
        SortOrder = track.SortOrder,
        Size = track.Size,
        DurationSeconds = ReadDurationSeconds(track),
        CreatedAt = track.CreatedAt,
        StreamUrl = $"/api/music/tracks/{track.Id}/stream"
    };

    private double? ReadDurationSeconds(MusicTrack track)
    {
        var path = AudioPath(track.StoredFileName);
        if (!File.Exists(path)) return null;
        try
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            if (Path.GetExtension(path).Equals(".wav", StringComparison.OrdinalIgnoreCase))
            {
                using var reader = new BinaryReader(stream);
                if (reader.ReadUInt32() != 0x46464952) return null;
                _ = reader.ReadUInt32();
                if (reader.ReadUInt32() != 0x45564157) return null;
                uint byteRate = 0;
                while (stream.Position + 8 <= stream.Length)
                {
                    var chunkId = reader.ReadUInt32();
                    var chunkSize = reader.ReadUInt32();
                    var next = stream.Position + chunkSize + (chunkSize & 1);
                    if (chunkId == 0x20746d66 && chunkSize >= 16)
                    {
                        stream.Position += 8;
                        byteRate = reader.ReadUInt32();
                    }
                    else if (chunkId == 0x61746164 && byteRate > 0)
                    {
                        return chunkSize / (double)byteRate;
                    }
                    if (next > stream.Length) break;
                    stream.Position = next;
                }
                return null;
            }

            if (!Path.GetExtension(path).Equals(".mp3", StringComparison.OrdinalIgnoreCase)) return null;
            var header = new byte[Math.Min(65536, (int)stream.Length)];
            stream.ReadExactly(header);
            var offset = 0;
            if (header.Length >= 10 && header[0] == 'I' && header[1] == 'D' && header[2] == '3')
            {
                offset = 10 + ((header[6] & 0x7f) << 21) + ((header[7] & 0x7f) << 14) + ((header[8] & 0x7f) << 7) + (header[9] & 0x7f);
                if ((header[5] & 0x10) != 0) offset += 10;
            }

            int[] mpeg1Layer3Bitrates = [0, 32, 40, 48, 56, 64, 80, 96, 112, 128, 160, 192, 224, 256, 320, 0];
            int[] mpeg2Layer3Bitrates = [0, 8, 16, 24, 32, 40, 48, 56, 64, 80, 96, 112, 128, 144, 160, 0];
            int[] sampleRates = [44100, 48000, 32000];
            for (var i = Math.Max(0, offset); i + 4 <= header.Length; i++)
            {
                if (header[i] != 0xff || (header[i + 1] & 0xe0) != 0xe0) continue;
                var version = (header[i + 1] >> 3) & 3;
                var layer = (header[i + 1] >> 1) & 3;
                var bitrateIndex = (header[i + 2] >> 4) & 15;
                var sampleRateIndex = (header[i + 2] >> 2) & 3;
                if (version == 1 || layer != 1 || bitrateIndex is 0 or 15 || sampleRateIndex == 3) continue;
                var sampleRate = sampleRates[sampleRateIndex] / (version == 3 ? 1 : version == 2 ? 2 : 4);
                var bitrate = (version == 3 ? mpeg1Layer3Bitrates : mpeg2Layer3Bitrates)[bitrateIndex];
                var channelMode = (header[i + 3] >> 6) & 3;
                var hasCrc = (header[i + 1] & 1) == 0;
                var sideInfoSize = version == 3 ? (channelMode == 3 ? 17 : 32) : (channelMode == 3 ? 9 : 17);
                var xing = i + 4 + (hasCrc ? 2 : 0) + sideInfoSize;
                if (xing + 12 <= header.Length &&
                    (header[xing..(xing + 4)].SequenceEqual("Xing"u8) || header[xing..(xing + 4)].SequenceEqual("Info"u8)))
                {
                    var flags = BinaryPrimitives.ReadUInt32BigEndian(header.AsSpan(xing + 4, 4));
                    if ((flags & 1) != 0)
                    {
                        var frames = BinaryPrimitives.ReadUInt32BigEndian(header.AsSpan(xing + 8, 4));
                        if (frames > 0) return frames * (version == 3 ? 1152d : 576d) / sampleRate;
                    }
                }
                return stream.Length * 8d / (bitrate * 1000d);
            }
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
        return null;
    }
}
