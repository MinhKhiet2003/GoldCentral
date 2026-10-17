using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Base.Migrations
{
    /// <inheritdoc />
    public partial class AddWebsitePlatform : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_PortalApp_Platform",
                table: "PortalApp");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PortalApp_Platform",
                table: "PortalApp",
                sql: "\"Platform\" IN ('system', 'facebook', 'zalo', 'tiktok', 'youtube', 'website')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_PortalApp_Platform",
                table: "PortalApp");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PortalApp_Platform",
                table: "PortalApp",
                sql: "\"Platform\" IN ('system', 'facebook', 'zalo', 'tiktok', 'youtube')");
        }
    }
}
