using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Base.Migrations
{
    /// <inheritdoc />
    public partial class GoldPortalApplications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PortalApp",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    Category = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Platform = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Url = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ImageUrl = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IsVisible = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    Clicks = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortalApp", x => x.Id);
                    table.CheckConstraint("CK_PortalApp_Category", "\"Category\" IN ('vaccination', 'dermatology', 'general', 'office')");
                    table.CheckConstraint("CK_PortalApp_Clicks", "\"Clicks\" >= 0");
                    table.CheckConstraint("CK_PortalApp_Order", "\"Order\" BETWEEN 0 AND 10000");
                    table.CheckConstraint("CK_PortalApp_Platform", "\"Platform\" IN ('system', 'facebook', 'zalo', 'tiktok', 'youtube')");
                });

            migrationBuilder.InsertData(
                table: "PortalApp",
                columns: new[] { "Id", "Category", "Description", "ImageUrl", "IsVisible", "Name", "Order", "Platform", "Url" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000001"), "vaccination", "Quản lý lịch tiêm, hồ sơ khách hàng và dịch vụ tiêm chủng.", null, true, "Gold Tiêm chủng", 0, "system", null },
                    { new Guid("00000000-0000-0000-0000-000000000002"), "dermatology", "Kết nối hệ thống chăm sóc da và điều trị da liễu.", null, true, "Gold Da liễu", 1, "system", null },
                    { new Guid("00000000-0000-0000-0000-000000000003"), "general", "Quản lý khám chữa bệnh và chăm sóc sức khỏe toàn diện.", null, true, "Gold Phòng khám", 2, "system", null },
                    { new Guid("00000000-0000-0000-0000-000000000004"), "office", "Không gian làm việc, tài liệu và phối hợp giữa các phòng ban.", null, true, "Gold Văn phòng", 3, "system", null },
                    { new Guid("00000000-0000-0000-0000-000000000005"), "vaccination", "Cập nhật kiến thức vắc xin và hoạt động tại trung tâm.", null, true, "Tiêm chủng Gold Ninh Bình", 4, "facebook", null },
                    { new Guid("00000000-0000-0000-0000-000000000006"), "dermatology", "Chia sẻ hành trình chăm sóc da và những câu chuyện làm đẹp.", null, true, "Gold Skin & Beauty", 5, "facebook", null },
                    { new Guid("00000000-0000-0000-0000-000000000007"), "general", "Kết nối khách hàng, tư vấn và cập nhật thông tin dịch vụ.", null, true, "Gold Ninh Bình Official", 6, "zalo", null },
                    { new Guid("00000000-0000-0000-0000-000000000008"), "general", "Khám phá kiến thức sức khỏe qua những video ngắn.", null, true, "Một phút khỏe cùng Gold", 7, "tiktok", null },
                    { new Guid("00000000-0000-0000-0000-000000000009"), "general", "Video tư vấn chuyên gia và hoạt động trong hệ sinh thái Gold.", null, true, "Y tế Toàn Cầu Gold", 8, "youtube", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_PortalApp_Category_Platform",
                table: "PortalApp",
                columns: new[] { "Category", "Platform" });

            migrationBuilder.CreateIndex(
                name: "IX_PortalApp_IsVisible_Order",
                table: "PortalApp",
                columns: new[] { "IsVisible", "Order" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PortalApp");
        }
    }
}
