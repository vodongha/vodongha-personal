using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace vodongha.Migrations
{
    /// <inheritdoc />
    public partial class AddAISkills : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Skills",
                columns: new[] { "Id", "Category", "Icon", "Name", "Order", "Proficiency" },
                values: new object[,]
                {
                    { 14, "AI", "devicon-python-plain", "Claude (Anthropic)", 14, 90 },
                    { 15, "AI", "devicon-github-plain", "GitHub Copilot", 15, 85 },
                    { 16, "AI", "devicon-tensorflow-plain", "Prompt Engineering", 16, 80 },
                    { 17, "AI", "devicon-vscode-plain", "AI-Assisted Dev", 17, 85 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 17);
        }
    }
}
