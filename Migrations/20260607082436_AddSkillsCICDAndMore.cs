using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace vodongha.Migrations
{
    /// <inheritdoc />
    public partial class AddSkillsCICDAndMore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Skills",
                columns: new[] { "Id", "Category", "Icon", "Name", "Order", "Proficiency" },
                values: new object[,]
                {
                    { 22, "DevOps", "devicon-githubactions-plain", "CI/CD", 22, 80 },
                    { 23, "DevOps", "devicon-linux-plain", "Linux / Bash", 23, 75 },
                    { 24, "DevOps", "devicon-flyio-plain", "Fly.io", 24, 72 },
                    { 25, "Backend", "devicon-dotnetcore-plain", "Entity Framework", 25, 85 },
                    { 26, "Backend", "devicon-dotnetcore-plain", "SignalR", 26, 78 },
                    { 27, "Backend", "devicon-elasticsearch-plain", "Elasticsearch", 27, 70 },
                    { 28, "Backend", "devicon-dotnetcore-plain", "Hangfire", 28, 75 },
                    { 29, "Frontend", "devicon-sass-plain", "SCSS / Sass", 29, 82 },
                    { 30, "Frontend", "devicon-bootstrap-plain", "Bootstrap", 30, 78 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 30);
        }
    }
}
