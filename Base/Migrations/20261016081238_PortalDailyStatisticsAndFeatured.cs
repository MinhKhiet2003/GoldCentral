using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Base.Migrations
{
    /// <inheritdoc />
    public partial class PortalDailyStatisticsAndFeatured : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFeatured",
                table: "PortalApp",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "PortalAppDailyClick",
                columns: table => new
                {
                    AppId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Clicks = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortalAppDailyClick", x => new { x.AppId, x.Date });
                    table.CheckConstraint("CK_PortalAppDailyClick_Clicks", "\"Clicks\" > 0");
                    table.ForeignKey(
                        name: "FK_PortalAppDailyClick_PortalApp_AppId",
                        column: x => x.AppId,
                        principalTable: "PortalApp",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PortalAppDailyClick_Date",
                table: "PortalAppDailyClick",
                column: "Date");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PortalAppDailyClick");

            migrationBuilder.DropColumn(
                name: "IsFeatured",
                table: "PortalApp");
        }
    }
}

