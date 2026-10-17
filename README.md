# Base API

Base API là nền tảng .NET 10 cho các dự án cần quản lý người dùng, xác thực JWT, phân quyền, file, thông báo và nhật ký hoạt động. Dự án dùng ASP.NET Core, Entity Framework Core và PostgreSQL.

## Khởi động dự án

1. Cập nhật `Base/appsettings.json` với chuỗi kết nối PostgreSQL và cấu hình JWT.
2. Dùng database mới hoặc trống nếu database cũ còn lịch sử migration trước khi dự án được tạo lại.
3. Chạy tại thư mục chứa `Base.slnx`:

```powershell
dotnet ef database update --project Base/Base.csproj
dotnet run --project Base/Base.csproj --launch-profile https
```

Swagger chạy tại `https://localhost:7088/swagger` trong môi trường Development.

Migration tạo sẵn tài khoản quản trị đầu tiên:

```json
{
  "username": "admin",
  "password": "admin"
}
```

Tài khoản này có role `admin`. Mật khẩu trong database được băm, không được lưu dạng văn bản.

## Cấu trúc thư mục

```text
Base/
├── Authorization/            Role, permission và handler kiểm tra quyền
├── Common/                   Thành phần dùng chung, audit context, response writer
├── Controllers/              Các endpoint HTTP
├── Data/                     DbContext, factory migration, seed dữ liệu
├── Entities/                 Model ánh xạ bảng database
├── Exceptions/               Lỗi nghiệp vụ ApiException
├── Extensions/               Cấu hình API, validation, OpenAPI
├── Filters/                  Lọc response an toàn cho audit log
├── Middleware/               Nhật ký request API
├── Migrations/               Lịch sử thay đổi database
├── Models/
│   ├── Requests/             DTO client gửi lên
│   └── Responses/            DTO API trả về
├── Repositories/             Truy cập dữ liệu
│   └── IRepositories/        Interface repository
├── Services/                 Nghiệp vụ
│   └── IServices/            Interface service
└── wwwroot/uploads/          File đã upload
```

Request và Response được tách thành từng file. Không trả Entity trực tiếp cho client.

## Luồng xử lý

```text
Client → Controller → IService → Service → IRepository → Repository → AppDbContext → PostgreSQL
```

Ví dụ `PUT /api/accounts/me`:

1. JWT kiểm tra token, thời hạn và trạng thái tài khoản.
2. Permission handler kiểm tra quyền `users.profile.update`.
3. Controller lấy ID tài khoản từ claim `sub` và gọi service.
4. Service kiểm tra nghiệp vụ, tạo snapshot trước/sau thay đổi và lưu qua repository.
5. Controller trả `ApiResponse<UserResponse>`.
6. Middleware ghi nhật ký request và audit data.

Controller chỉ xử lý HTTP; Service chứa nghiệp vụ; Repository chỉ truy cập database.

## Xác thực và phiên đăng nhập

`POST /api/accounts/login` trả access token. Client gửi token cho các API bảo vệ:

```http
Authorization: Bearer <accessToken>
```

Access token có thời hạn 30 phút. Mỗi request xác minh token, tài khoản có tồn tại/đang hoạt động và lấy lại role hiện tại từ database. Vì vậy token cũ không giữ quyền sau khi tài khoản bị khóa hoặc bị hạ role.

Đăng nhập và `POST /api/auth/refresh` trả thêm refresh token có thời hạn 14 ngày. Refresh token chỉ lưu hash trong database và được xoay vòng sau mỗi lần làm mới. Access token chứa mã phiên và mọi API kiểm tra phiên đó còn hiệu lực, vì vậy `POST /api/auth/logout` thu hồi ngay access token của một thiết bị; `POST /api/auth/logout-all` thu hồi ngay toàn bộ phiên của tài khoản đang đăng nhập. Đổi hoặc đặt lại mật khẩu cũng thu hồi tất cả phiên.

`POST /api/auth/forgot-password` luôn trả thông báo giống nhau để không làm lộ email có tồn tại hay không. Khi chạy Development, response có `resetToken` để test; môi trường Production không trả token và cần tích hợp email/SMS để gửi token cho người dùng. Token đặt lại mật khẩu có thời hạn 30 phút và chỉ dùng một lần.

## Role và permission

Role trong bảng `"User"` gồm `admin` và `user`. `Permission` là danh mục quyền; `RolePermission` ánh xạ role với permission. Admin là super-admin và luôn có tất cả quyền; chỉ admin mới được gọi API quản trị role/permission.

| Permission | admin | user |
|---|---:|---:|
| `users.profile.read`, `users.profile.update` | Có | Có |
| `users.list`, `users.read`, `users.update`, `users.delete` | Có | Không |
| `files.upload` | Có | Có |
| `notifications.read`, `notifications.update` | Có | Có |
| `system-logs.read`, `roles.manage`, `permissions.manage` | Có | Không |
| `settings.manage`, `dashboard.read` | Có | Không |

Đăng ký công khai luôn tạo role `user`. Admin khóa/mở khóa qua API cập nhật tài khoản và đổi role qua endpoint riêng. Hệ thống chặn admin tự hạ quyền và chặn hạ quyền admin cuối cùng đang hoạt động.

Khi thêm một permission: thêm hằng số tại `Authorization/AppPermissions.cs`, seed ở `AppDbContext`, tạo policy tại `Program.cs`, rồi gắn `[Authorize(Policy = ...)]` vào endpoint.

## API hiện có

| Method | Endpoint | Mục đích |
|---|---|---|
| POST | `/api/accounts/register` | Đăng ký tài khoản user |
| POST | `/api/accounts/login` | Đăng nhập và nhận JWT |
| POST | `/api/auth/refresh` | Làm mới access token và refresh token |
| POST | `/api/auth/logout` | Đăng xuất một thiết bị bằng refresh token |
| POST | `/api/auth/logout-all` | Đăng xuất tất cả thiết bị của tôi |
| POST | `/api/auth/forgot-password` | Tạo yêu cầu quên mật khẩu |
| POST | `/api/auth/reset-password` | Đặt lại mật khẩu bằng token |
| POST | `/api/auth/change-password` | Đổi mật khẩu khi đã đăng nhập |
| GET / PUT | `/api/accounts/me` | Xem/cập nhật thông tin cá nhân |
| GET | `/api/accounts` | Admin: danh sách, tìm kiếm, lọc, phân trang |
| GET / PUT / DELETE | `/api/accounts/{id}` | Admin: xem, cập nhật, xóa tài khoản |
| PUT | `/api/accounts/{id}/role` | Admin: gán role an toàn cho tài khoản |
| GET | `/api/permissions` | Admin: danh sách permission |
| GET | `/api/roles` | Admin: danh sách role và permission |
| PUT | `/api/roles/user/permissions` | Admin: thay permission cho role user |
| POST | `/api/files/upload` | Upload JPG, PNG, PDF, TXT |
| POST | `/api/accounts/me/avatar` | Upload và gán avatar JPG/PNG/JPEG cho tôi |
| GET | `/api/files/{id}` | Xem/tải file của tôi; admin xem mọi file |
| DELETE | `/api/files/{id}` | Xóa file của tôi; admin xóa mọi file |
| GET | `/api/notifications` | Danh sách thông báo của tôi |
| GET | `/api/notifications/unread-count` | Số thông báo chưa đọc |
| PUT | `/api/notifications/{id}/read` | Đánh dấu một thông báo đã đọc |
| PUT | `/api/notifications/read-all` | Đánh dấu tất cả đã đọc |
| GET | `/api/dashboard` | Admin: tổng quan người dùng và đăng nhập gần đây |
| GET | `/api/system-logs` | Admin: danh sách nhật ký |
| GET | `/api/system-logs/{id}` | Admin: chi tiết nhật ký và audit data |

Danh sách tài khoản hỗ trợ `pageNumber`, `pageSize`, `keyword`, `role`, `isActive`. `pageSize` từ 1 đến 100.

## Phản hồi API

Mọi API nghiệp vụ dùng cùng cấu trúc:

```json
{
  "statusCode": 200,
  "success": true,
  "message": "Thao tác thành công.",
  "data": {},
  "errors": null
}
```

Lỗi validation có `errors` theo tên trường và thông báo tiếng Việt. HTTP status luôn trùng `statusCode` trong body. Danh sách dùng `ApiResponse<PagedResult<T>>` với `items`, `pageNumber`, `pageSize`, `totalItems`, `totalPages`.

Swagger ẩn danh sách response mẫu để giao diện gọn. Kết quả thực tế sau khi bấm **Execute** vẫn hiển thị đầy đủ.

## Quản lý file

`POST /api/files/upload` dùng `multipart/form-data`, trường có tên `file`.

- Tối đa 10 MB, mỗi request một file.
- Chỉ nhận `.jpg`, `.jpeg`, `.png`, `.pdf`, `.txt`.
- Ảnh/PDF được kiểm tra chữ ký nội dung.
- File được đổi tên bằng GUID và lưu tại `wwwroot/uploads`.
- Metadata lưu trong `UploadedFile`; nếu lưu metadata lỗi, file vừa tạo được xóa.

API trả URL tương đối như `/uploads/<guid>.png`. Client dùng URL đó để cập nhật `avatarUrl`. URL file công khai; ảnh hiển thị trực tiếp, PDF/TXT tải xuống. Chưa tích hợp quét virus.

`POST /api/accounts/me/avatar` upload và gán avatar trong một request; chỉ nhận JPG, JPEG, PNG.

## Cấu hình hệ thống

Admin quản lý các cấu hình chung qua `GET /api/system-settings` và `PUT /api/system-settings/{key}`. Key chỉ gồm chữ/số/dấu chấm/gạch dưới/gạch ngang.

Nên dùng các key thống nhất: `system.name`, `system.logoUrl`, `system.faviconUrl`, `organization.name`, `organization.taxCode`, `organization.address`, `organization.phone`, `organization.email`, `organization.website`. Logo và favicon nên upload trước, sau đó lưu URL trả về vào cấu hình.

Các key này đã được seed mặc định khi chạy migration. Chỉ `system.name` có giá trị `Base API`; các key còn lại để trống để admin cập nhật.

## CORS

API cho phép frontend từ `http://localhost:3000` và `http://localhost:5173`. Khi frontend dùng domain khác, thêm domain đó vào `Cors:AllowedOrigins` trong cấu hình môi trường. Không dùng `AllowAnyOrigin` cho API có JWT.

## Gửi email quên mật khẩu

`POST /api/auth/forgot-password` gửi email qua SMTP sau khi tìm thấy tài khoản. Cấu hình mẫu dùng Gmail tại `Base/appsettings.json`:

```json
"Mail": {
  "Host": "smtp.gmail.com",
  "Port": 587,
  "UseSsl": true,
  "Username": "your-email@gmail.com",
  "Password": "gmail-app-password",
  "FromEmail": "your-email@gmail.com",
  "FromName": "Base API",
  "PasswordResetUrl": "http://localhost:5173/reset-password?token={token}"
}
```

Với Gmail, bật xác minh hai bước rồi tạo **App Password**; không dùng mật khẩu đăng nhập Gmail. Không commit mật khẩu SMTP. Khi phát triển local, cấu hình bằng User Secrets:

```powershell
dotnet user-secrets set "Mail:Username" "your-email@gmail.com" --project Base/Base.csproj
dotnet user-secrets set "Mail:Password" "gmail-app-password" --project Base/Base.csproj
dotnet user-secrets set "Mail:FromEmail" "your-email@gmail.com" --project Base/Base.csproj
```

## Nhật ký hệ thống và audit

Mỗi request `/api` được ghi vào `SystemLog`: người thực hiện, method, path, status code, IP, trace ID, thời gian xử lý và thời điểm.

Với các thao tác thay đổi dữ liệu thành công, log có thêm:

| Trường | Ý nghĩa |
|---|---|
| `action` | Create, Update, Delete, Login, Upload |
| `entityType`, `entityId` | Bản ghi bị tác động |
| `beforeData` | Snapshot trước khi sửa/xóa |
| `afterData` | Snapshot sau khi thêm/sửa/upload |
| `responseData` | Dữ liệu response đã được cho phép ghi log |

Không lưu password, password hash, access token, request body, query string hay nội dung file. Danh sách log trả metadata gọn; gọi `GET /api/system-logs/{id}` để xem snapshot chi tiết.

Ghi log dùng scope database riêng. Nếu ghi log thất bại, nghiệp vụ chính không bị thất bại theo.

## Database và migration

Giữ toàn bộ migration trong source control và áp dụng theo thứ tự. Khi thay đổi Entity hoặc mapping:

```powershell
dotnet ef migrations add TenMigration --project Base/Base.csproj
dotnet ef database update --project Base/Base.csproj
```

Không sửa/xóa migration đã áp dụng ở môi trường dùng chung. Để làm lại dữ liệu phát triển, sử dụng database trống.

## Phần nền đã chuẩn bị để phát triển tiếp

Danh sách yêu cầu đã có phần nền database/permission nhưng chưa có endpoint nghiệp vụ hoàn chỉnh:

- Tích hợp email/SMS để gửi token quên mật khẩu trong môi trường Production.
- Tạo role động ngoài `admin` và `user` nếu dự án cần nhiều cấp vai trò.
- Admin tạo tài khoản riêng.
- Gửi thông báo từ nghiệp vụ/admin; hiện API mới quản lý thông báo đã có.
- Xem/tải/xóa file riêng theo quyền; hiện đã có upload và static URL công khai.

Khi triển khai các phần này, giữ đúng luồng Controller → Service → Repository → DbContext, tạo DTO riêng và bổ sung audit snapshot an toàn.

## Nhập tài khoản từ Excel

Admin đăng nhập rồi gọi GET /api/accounts/import/template để tải mẫu .xlsx.
Điền dữ liệu ở sheet đầu tiên, dòng 1 là tên cột. POST /api/accounts/import
dùng multipart/form-data, trường file, kèm Bearer token admin.

Cột bắt buộc: Username, Password, FullName, Email.
Cột tùy chọn: PhoneNumber, DateOfBirth, Gender, Address.
Password có 12–128 ký tự. DateOfBirth dùng yyyy-MM-dd hoặc ô ngày Excel.
Gender: Male, Female, Other, PreferNotToSay. Định dạng PhoneNumber là Text
để giữ số 0 đầu. Mỗi file tối đa 5 MB và 1.000 dòng, không chứa công thức.

Chỉ tạo tài khoản mới, role user và đang hoạt động. Không cập nhật tài khoản cũ.
Dòng sai dữ liệu hoặc trùng username/email bị bỏ qua; các dòng hợp lệ được lưu
trong một lần SaveChanges. Response trả totalRows, importedCount, failedCount,
errors (row là số dòng Excel, message là lý do). Không trả mật khẩu.
Không cần migration mới.

Import bỏ qua dòng trùng username, email hoặc PhoneNumber trong database hoặc các
dòng hợp lệ trước đó trong file. Username/email so sánh không phân biệt hoa thường;
số điện thoại bỏ khoảng trắng, dấu +, -, ., ngoặc. Số điện thoại trống không tính trùng.
Response: importedCount (đã thêm), duplicateCount (trùng), invalidCount (sai dữ liệu).
duplicates chứa danh sách trùng và lý do. duplicateDownloadUrl dùng GET với Bearer
token admin để tải Excel danh sách trùng, không có mật khẩu; hiệu lực 30 phút,
hết hạn khi khởi động lại server. Khi có nhiều instance, dùng cùng instance để tải.

## Gold Central — Cổng kết nối hệ sinh thái

Giao diện MVC tại `/`, dùng View Razor và CSS/JavaScript thuần, responsive cho điện thoại và máy tính.
Giao diện tối giản ưu tiên truy cập hệ thống, màu xanh và vàng theo logo Gold được cung cấp; không cần cài frontend framework.

```powershell
dotnet run --project Base/Base.csproj --no-launch-profile --urls http://localhost:5088
```

Mở http://localhost:5088. Danh sách mẫu có 9 thẻ thuộc 4 nhóm chuyên môn và các nền tảng
nội bộ, Facebook, Zalo, TikTok, YouTube. Tên các kênh xã hội là nội dung minh họa,
cần thay bằng kênh chính thức. URL mặc định để trống vì chưa được cung cấp.
Thẻ chưa có URL hiển thị “Chờ cấu hình”. Ảnh lỗi hoặc thiếu có biểu tượng thay thế.

- Trang chủ gồm Yêu thích ở trên và Nổi bật ở dưới; Nổi bật chỉ gồm ứng dụng được admin bật IsFeatured và IsVisible, theo thứ tự quản trị thiết lập. Lọc lĩnh vực và nền tảng bằng select.
- Tìm tên/mô tả có hỗ trợ tìm không dấu, lọc lĩnh vực. Thẻ có URL được nhấp trên toàn bộ bề mặt để truy cập.
- Responsive: 3 cột trên desktop, 2 cột trên tablet và 1 cột trên điện thoại; logo sử dụng nguyên ảnh gốc, căn khung bằng CSS.
- Yêu thích lưu riêng trong localStorage của trình duyệt, không đồng bộ tài khoản.
- Trong quản trị: sắp xếp A–Z, lượt nhấp hoặc thứ tự do quản trị thiết lập trong form sửa thẻ.
- Quản trị truy cập trực tiếp `/admin`, không có menu quản trị trên trang chủ. Đăng nhập thành công mới tải danh sách quản trị và mở các thao tác. Token chỉ giữ trong bộ nhớ trang.
- Thêm/sửa/xóa, ẩn/hiện và thống kê yêu cầu role admin tại API, không chỉ tại giao diện.
- Thẻ dùng ảnh bìa ngang lớn 16:9 (object-fit cover), URL ảnh hoặc upload PNG/JPG qua API file hiện có. Thiếu/lỗi ảnh sẽ dùng logo Gold. Toàn bộ thẻ có URL đều nhấp được, trừ nút yêu thích và các nút chỉnh sửa/ẩn/xóa.
- Thống kê gồm ngày, tuần (thứ Hai–Chủ nhật), tháng lịch và tổng, theo giờ Việt Nam UTC+7; admin chọn ngày để xem kỳ tương ứng. Đây là lượt nhấp từ cổng, không phải số người dùng duy nhất hoặc xác nhận truy cập thành công ở hệ thống đích.
- Cổng hiện cho phép xem danh sách thẻ công khai; quyền đăng nhập bên trong từng ứng dụng do ứng dụng đích quản lý.
  Chưa có SSO hoặc phân quyền xem từng thẻ theo phòng ban.

Luồng: PortalController → IPortalService → PortalService → IPortalRepository → PortalRepository.
Repository dùng AppDbContext/PostgreSQL và bảng `PortalApp`, tạo bởi migration `GoldPortalApplications`. Lượt nhấp được tăng bằng SQL để giữ đúng dữ liệu khi nhiều request cùng cập nhật.

API cổng:

| Method | Endpoint | Quyền |
|---|---|---|
| GET | `/api/portal/apps` | Công khai, chỉ thẻ hiện |
| GET | `/api/portal/manage` | Admin, gồm thẻ ẩn |
| GET | `/api/portal/statistics?date=2026-10-16` | Admin, thống kê ngày/tuần/tháng/tổng |
| POST | `/api/portal/apps` | Admin |
| PUT / DELETE | `/api/portal/apps/{id}` | Admin |
| POST | `/api/portal/apps/{id}/visit` | Công khai, đếm và trả URL thẻ hiện |

Kiểm tra model EF và nghiệp vụ URL, ẩn thẻ, xóa, xử lý liên kết vừa thay đổi bằng repository giả (không kết nối database):

```powershell
dotnet run --project tests/PortalSmoke/PortalSmoke.csproj
```

## Migration và cổng chạy Gold Central

Chạy từ thư mục `D:\Minh Khiet\Project\GoldCentral`:

```powershell
dotnet ef database update --project .\Base\Base\Base.csproj
dotnet run --project .\Base\Base\Base.csproj --launch-profile https
```

Cấu hình `ConnectionStrings:DefaultConnection` trong `Base/Base/appsettings.json` hoặc biến môi trường
trước khi chạy migration. Không tự động áp dụng migration khi ứng dụng khởi động.
`database update` áp dụng toàn bộ migration còn thiếu, bao gồm bảng nền cho tài khoản/đăng nhập
và bảng PortalApp. Database mới nhận 9 thẻ mẫu với URL để trống.

Profile `https`: https://localhost:7088 (giao diện), https://localhost:7088/swagger (API), kèm HTTP 5088.
Profile `http`: http://localhost:5088. Cổng 7015 đã được thay trong launchSettings.json.
IIS Express dùng HTTP 52888 và SSL 44388.

Nếu chỉ muốn HTTP:

```powershell
dotnet run --project .\Base\Base\Base.csproj --launch-profile http
```

Có thể dùng `Base/GoldCentral.migrations.sql` thay cho lệnh EF để cập nhật một database đã tồn tại.
Đây là script idempotent của toàn bộ chuỗi migration, chỉ áp dụng các migration chưa có trong lịch sử EF.
Script không tự tạo database. Không cần chạy cả script SQL và lệnh EF.

## Thống kê theo kỳ

Migration `PortalDailyStatisticsAndFeatured` thêm `PortalApp.IsFeatured` và bảng `PortalAppDailyClick`.
Chạy trong Package Manager Console của Visual Studio khi đã dừng ứng dụng:

```powershell
Update-Database -Project Base -StartupProject Base
```

Migration chỉ thêm dữ liệu/cấu trúc mới, giữ nguyên tổng Clicks và cấu hình thẻ hiện có.
Giao diện hiện tại đã bỏ mục Nổi bật. Cột IsFeatured được giữ để tương thích dữ liệu cũ.
Thứ tự hiển thị dùng trường Order hiện có. Thẻ bị ẩn không xuất hiện trên trang chủ.

Mỗi lần nhấp thành công sẽ cập nhật tổng và bucket ngày trong cùng transaction PostgreSQL.
Khóa (AppId, Date) cùng upsert bảo đảm nhiều request ghi vào cùng ngày không mất lượt.
Không ghi IP hoặc định danh người dùng trong bảng thống kê này.
Chọn ngày trên trang thống kê để xem ngày đó, tuần chứa ngày đó, tháng chứa ngày đó và tổng mọi thời điểm.
Các bộ lọc lĩnh vực/nền tảng/từ khóa áp dụng cả bảng và thẻ tổng hợp.
Lượt cũ trước migration chỉ nằm trong Tổng vì không có lịch sử ngày để phân bổ chính xác.
Xóa ứng dụng sẽ xóa cả thống kê theo ngày của ứng dụng đó.

Yêu thích vẫn lưu trong localStorage dưới khóa `gold-favorites` (mảng ID ứng dụng).
Dữ liệu riêng cho origin/protocol/port và profile trình duyệt, không hết hạn theo phiên;
xóa dữ liệu trình duyệt sẽ mất, không đồng bộ giữa máy/thiết bị/tài khoản. Không chuyển sang server trong thay đổi này.

Kiểm thử tích hợp đầy đủ (chạy từ thư mục GoldCentral):

```powershell
dotnet run --project Base/tests/PortalSmoke/PortalSmoke.csproj --configuration Release -- --postgres
```

Bài test dùng kết nối cấu hình để tạo database tạm có tiền tố `goldportal_smoke_`, chạy toàn bộ migration,
kiểm tra thống kê và lượt nhấp đồng thời rồi tự xóa database tạm. Không sửa database GoldCentral.
Tài khoản PostgreSQL cho kiểm thử cần quyền tạo database. Biến môi trường GOLD_PORTAL_TEST_CONNECTION
có thể chỉ định riêng server/tài khoản kiểm thử; không ghi giá trị này vào source.
Thêm `--serve` để giữ bản xem trước test tại localhost:5090 cho đến khi nhấn Enter, sau đó tự dọn dữ liệu test.

## Danh mục đầy đủ theo đơn vị

Trang chủ có Tất cả ứng dụng bên dưới Yêu thích; hiển thị mọi thẻ IsVisible,
kể cả IsFeatured=false. Các tab đơn vị sử dụng Category hiện có (Tiêm chủng, Da liễu & Thẩm mỹ,
Khám chữa bệnh, Văn phòng & Nội bộ). Trong admin, chọn trường Hệ thống để gán thẻ vào nhóm tương ứng.
Mỗi tab lọc theo Tất cả loại / Hệ thống / Mạng xã hội; khi chọn mạng xã hội có thêm bộ lọc nền tảng.
Các tab có số lượng, hỗ trợ phím mũi tên/Home/End và cuộn ngang trên mobile.
Tìm kiếm áp dụng toàn trang; việc đổi tab/loại chỉ thay đổi danh mục đầy đủ, giữ các mục truy cập nhanh phía trên.
Đây là thay đổi giao diện, không thêm migration mới.

## Loại Website và logo Zalo

Migration AddWebsitePlatform mở rộng ràng buộc CK_PortalApp_Platform để hỗ trợ website,
không đổi hoặc xóa thẻ hiện có. Website được xếp cùng nhóm Mạng xã hội & Website,
có trong các select quản trị/bộ lọc và dùng icon quả địa cầu.
Logo Zalo dùng SVG từ trang chính thức: https://zalo-site.zadn.vn/_next/static/media/logo.a68785cd.svg
Bản local: Base/wwwroot/portal/icons/zalo.svg; không cần tải logo từ bên thứ ba khi mở trang.
Chạy Update-Database -Project Base -StartupProject Base rồi chạy lại ứng dụng.

Menu phòng ban hiển thị dọc bên trái danh mục Tất cả ứng dụng trên desktop từ 1100px,
đứng cố định trong khu vực danh mục khi cuộn. Bên phải là 3 cột thẻ.
Trên màn hình nhỏ hơn, bộ lọc chuyển sang hàng ngang cuộn được; hỗ trợ phím mũi tên theo hướng hiển thị.

Menu phòng ban hiện bắt đầu ngay đầu trang, ngang hàng tiêu đề/tìm kiếm, đứng bên trái toàn bộ nội dung.
Chọn phòng ban áp dụng cho Yêu thích và Tất cả ứng dụng; số lượng trên menu vẫn hiển thị các nhóm đầy đủ.
Lưới thẻ giữ 3 cột desktop. Trên màn hình nhỏ, bộ lọc nằm trên tiêu đề dưới dạng hàng ngang.

## Tin tức nội bộ

Menu trên cùng gồm Trang chủ và Tin tức (`/news`). Truy cập `/admin` để quản trị;
menu công khai không hiển thị liên kết quản trị. Trang chủ đã bỏ danh sách Nổi bật.

Admin có mục Quản lý tin tức: tạo, sửa, xóa, lưu nháp, xuất bản/ẩn bài, tải ảnh PNG/JPG,
gán phòng ban, tìm kiếm, lọc trạng thái và phân trang. Nội dung bài viết là văn bản thuần,
giữ xuống dòng và không thực thi HTML. Mọi người có thể đọc tin đã xuất bản tại `/news`
mà không cần đăng nhập; bài nháp và các thao tác quản lý chỉ dành cho vai trò admin.
Bộ lọc phòng ban là phân loại nội dung, không phải phân quyền theo phòng ban.
Trang đọc tin không sử dụng token hay lưu phiên đăng nhập.

Đã thêm migration `20261016100007_InternalNews` và cập nhật `GoldCentral.migrations.sql`.
Migration tạo bảng NewsArticle, không thay đổi thẻ ứng dụng hiện có. Dừng ứng dụng rồi chạy
trong Package Manager Console của Visual Studio:

```powershell
Update-Database -Project Base -StartupProject Base
```

Sau đó chạy lại ứng dụng. Kiểm thử `--postgres` kiểm tra thêm vòng đời bài nháp/xuất bản,
lọc, phân trang, URL ảnh và nội dung chi tiết trên database tạm; không cập nhật GoldCentral.

## Báo cáo biểu đồ và lượt đọc tin

Trang Thống kê truy cập có ba biểu đồ theo lĩnh vực, ứng dụng nội bộ và nền tảng xã hội/website.
Mỗi biểu đồ đổi được kỳ Ngày, Tuần, Tháng hoặc Tổng; áp dụng ngày đã chọn và các bộ lọc ứng dụng.
Bộ chọn ngày được trình bày đồng hàng, có trường nhập ngày theo giao diện trình duyệt.

Trang tin công khai ghi nhận một lượt đọc khi mở chi tiết bài đã xuất bản (bài nháp không được tính).
Số lượt đọc toàn thời gian hiển thị trên thẻ tin và trong danh sách Quản lý tin tức.
Migration `20261017010413_NewsArticleViewCount` thêm bộ đếm với giá trị ban đầu 0;
chạy `Update-Database -Project Base -StartupProject Base` trong Package Manager Console để áp dụng.
Không có lịch sử lượt đọc trước khi migration nên các bài cũ bắt đầu từ 0.
