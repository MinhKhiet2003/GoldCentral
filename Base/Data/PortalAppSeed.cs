using Base.Entities;
namespace Base.Data;

public static class PortalAppSeed
{
    public static List<PortalApp> Items()
    {
        string[][] rows = [
            ["Gold Tiêm chủng", "Quản lý lịch tiêm, hồ sơ khách hàng và dịch vụ tiêm chủng.", "vaccination", "system"],
            ["Gold Da liễu", "Kết nối hệ thống chăm sóc da và điều trị da liễu.", "dermatology", "system"],
            ["Gold Phòng khám", "Quản lý khám chữa bệnh và chăm sóc sức khỏe toàn diện.", "general", "system"],
            ["Gold Văn phòng", "Không gian làm việc, tài liệu và phối hợp giữa các phòng ban.", "office", "system"],
            ["Tiêm chủng Gold Ninh Bình", "Cập nhật kiến thức vắc xin và hoạt động tại trung tâm.", "vaccination", "facebook"],
            ["Gold Skin & Beauty", "Chia sẻ hành trình chăm sóc da và những câu chuyện làm đẹp.", "dermatology", "facebook"],
            ["Gold Ninh Bình Official", "Kết nối khách hàng, tư vấn và cập nhật thông tin dịch vụ.", "general", "zalo"],
            ["Một phút khỏe cùng Gold", "Khám phá kiến thức sức khỏe qua những video ngắn.", "general", "tiktok"],
            ["Y tế Toàn Cầu Gold", "Video tư vấn chuyên gia và hoạt động trong hệ sinh thái Gold.", "general", "youtube"]
        ];
        return rows.Select((r, i) => new PortalApp { Id = Guid.Parse($"00000000-0000-0000-0000-{i + 1:000000000000}"), Name = r[0], Description = r[1], Category = r[2], Platform = r[3], Order = i }).ToList();
    }
}

