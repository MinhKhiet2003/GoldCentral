using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Base.Migrations
{
    /// <inheritdoc />
    public partial class SplitGeneralIntoRehabilitationAndWellness : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_PortalApp_Category",
                table: "PortalApp");

            migrationBuilder.DropCheckConstraint(
                name: "CK_NewsArticle_Department",
                table: "NewsArticle");

            migrationBuilder.Sql("UPDATE \"PortalApp\" SET \"Category\" = CASE WHEN \"Platform\" = 'system' THEN 'rehabilitation' ELSE 'wellness' END WHERE \"Category\" = 'general'");
            migrationBuilder.Sql("UPDATE \"NewsArticle\" SET \"Department\" = 'rehabilitation' WHERE \"Department\" = 'general'");

            migrationBuilder.UpdateData(
                table: "PortalApp",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000003"),
                columns: new[] { "Category", "Description", "Name" },
                values: new object[] { "rehabilitation", "Quản lý hoạt động phục hồi chức năng và chăm sóc sức khỏe.", "Gold Phục hồi chức năng" });

            migrationBuilder.UpdateData(
                table: "PortalApp",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000007"),
                column: "Category",
                value: "wellness");

            migrationBuilder.UpdateData(
                table: "PortalApp",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000008"),
                column: "Category",
                value: "wellness");

            migrationBuilder.UpdateData(
                table: "PortalApp",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000009"),
                column: "Category",
                value: "wellness");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PortalApp_Category",
                table: "PortalApp",
                sql: "\"Category\" IN ('vaccination', 'dermatology', 'rehabilitation', 'wellness', 'office')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_NewsArticle_Department",
                table: "NewsArticle",
                sql: "\"Department\" IN ('all', 'vaccination', 'dermatology', 'rehabilitation', 'wellness', 'office')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_PortalApp_Category",
                table: "PortalApp");

            migrationBuilder.DropCheckConstraint(
                name: "CK_NewsArticle_Department",
                table: "NewsArticle");

            migrationBuilder.Sql("UPDATE \"PortalApp\" SET \"Category\" = 'general' WHERE \"Category\" IN ('rehabilitation', 'wellness')");
            migrationBuilder.Sql("UPDATE \"NewsArticle\" SET \"Department\" = 'general' WHERE \"Department\" IN ('rehabilitation', 'wellness')");

            migrationBuilder.UpdateData(
                table: "PortalApp",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000003"),
                columns: new[] { "Category", "Description", "Name" },
                values: new object[] { "general", "Quản lý khám chữa bệnh và chăm sóc sức khỏe toàn diện.", "Gold Phòng khám" });

            migrationBuilder.UpdateData(
                table: "PortalApp",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000007"),
                column: "Category",
                value: "general");

            migrationBuilder.UpdateData(
                table: "PortalApp",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000008"),
                column: "Category",
                value: "general");

            migrationBuilder.UpdateData(
                table: "PortalApp",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000009"),
                column: "Category",
                value: "general");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PortalApp_Category",
                table: "PortalApp",
                sql: "\"Category\" IN ('vaccination', 'dermatology', 'general', 'office')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_NewsArticle_Department",
                table: "NewsArticle",
                sql: "\"Department\" IN ('all', 'vaccination', 'dermatology', 'general', 'office')");
        }
    }
}
