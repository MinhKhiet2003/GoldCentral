using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Base.Migrations
{
    /// <inheritdoc />
    public partial class NewsArticleViewCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ViewCount",
                table: "NewsArticle",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddCheckConstraint(
                name: "CK_NewsArticle_ViewCount",
                table: "NewsArticle",
                sql: "\"ViewCount\" >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_NewsArticle_ViewCount",
                table: "NewsArticle");

            migrationBuilder.DropColumn(
                name: "ViewCount",
                table: "NewsArticle");
        }
    }
}
