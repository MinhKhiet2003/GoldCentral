using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Base.Migrations
{
    /// <inheritdoc />
    public partial class InternalNews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NewsArticle",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Summary = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    Content = table.Column<string>(type: "character varying(30000)", maxLength: 30000, nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Department = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsArticle", x => x.Id);
                    table.CheckConstraint("CK_NewsArticle_Department", "\"Department\" IN ('all', 'vaccination', 'dermatology', 'general', 'office')");
                });

            migrationBuilder.CreateIndex(
                name: "IX_NewsArticle_Department_IsPublished",
                table: "NewsArticle",
                columns: new[] { "Department", "IsPublished" });

            migrationBuilder.CreateIndex(
                name: "IX_NewsArticle_IsPublished_PublishedAt",
                table: "NewsArticle",
                columns: new[] { "IsPublished", "PublishedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NewsArticle");
        }
    }
}
