using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarketingBlogApp.Migrations
{
    /// <inheritdoc />
    public partial class AddPhoneMumberToContactMessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "ContactMessages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "ContactMessages");
        }
    }
}
