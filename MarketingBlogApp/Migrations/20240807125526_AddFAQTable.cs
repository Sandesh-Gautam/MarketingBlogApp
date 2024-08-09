using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MarketingBlogApp.Migrations
{
    /// <inheritdoc />
    public partial class AddFAQTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FAQs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Question = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Answer = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FAQs", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "FAQs",
                columns: new[] { "Id", "Answer", "Question" },
                values: new object[,]
                {
                    { 1, "This site allows users to browse and interact with blog posts.", "What is the purpose of this site?" },
                    { 2, "You can create a new blog post by navigating to the 'My Posts' section if you are logged in.", "How can I create a new blog post?" },
                    { 3, "You can contact support using the 'Contact' page.", "How can I contact support?" },
                    { 4, "Use the search bar at the top of the blog post list to search for posts.", "How can I search for posts?" },
                    { 5, "The categories are listed in the sidebar. Click on a category to filter posts by that category.", "What categories are available?" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FAQs");
        }
    }
}
