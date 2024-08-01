using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarketingBlogApp.Migrations
{
    /// <inheritdoc />
    public partial class ManagerIssue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BlogPostId",
                table: "Warnings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CommentId",
                table: "Warnings",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BlogPostId",
                table: "Warnings");

            migrationBuilder.DropColumn(
                name: "CommentId",
                table: "Warnings");
        }
    }
}
