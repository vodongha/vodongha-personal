using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace VodonghaPersonal.Migrations
{
    /// <inheritdoc />
    public partial class AddBioEnAndFacebookSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 10,
                column: "Value",
                value: "/images/avatar.png");

            migrationBuilder.InsertData(
                table: "SiteSettings",
                columns: new[] { "Id", "Key", "Value" },
                values: new object[,]
                {
                    { 11, "BioEn", "I build modern web applications with .NET, Blazor, and PostgreSQL. Passionate about creating clean, efficient, and beautiful products." },
                    { 12, "Facebook", "" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 10,
                column: "Value",
                value: "/images/avatar.jpg");
        }
    }
}
