using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarketingBlogApp.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedBlogPostModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_BlogPosts_BlogPostId",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Categories_BlogPostId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "BlogPostId",
                table: "Categories");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BlogPostId",
                table: "Categories",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_BlogPostId",
                table: "Categories",
                column: "BlogPostId");

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_BlogPosts_BlogPostId",
                table: "Categories",
                column: "BlogPostId",
                principalTable: "BlogPosts",
                principalColumn: "Id");
        }
    }
}
