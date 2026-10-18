using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Base.Migrations
{
    /// <inheritdoc />
    public partial class DepartmentMusicLibrary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MusicTrack",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Artist = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    Department = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    OriginalFileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    StoredFileName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Size = table.Column<long>(type: "bigint", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MusicTrack", x => x.Id);
                    table.CheckConstraint("CK_MusicTrack_Department", "\"Department\" IN ('all', 'vaccination', 'dermatology', 'rehabilitation', 'wellness', 'office')");
                    table.CheckConstraint("CK_MusicTrack_Size", "\"Size\" > 0 AND \"Size\" <= 41943040");
                    table.CheckConstraint("CK_MusicTrack_SortOrder", "\"SortOrder\" BETWEEN 1 AND 10000");
                });

            migrationBuilder.CreateIndex(
                name: "IX_MusicTrack_CreatedAt",
                table: "MusicTrack",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_MusicTrack_Department_IsPublished_SortOrder",
                table: "MusicTrack",
                columns: new[] { "Department", "IsPublished", "SortOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MusicTrack");
        }
    }
}
