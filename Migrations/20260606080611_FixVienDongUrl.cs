using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VodonghaPersonal.Migrations
{
    /// <inheritdoc />
    public partial class FixVienDongUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Educations",
                keyColumn: "Id",
                keyValue: 2,
                column: "WebsiteUrl",
                value: "https://www.viendong.edu.vn");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Educations",
                keyColumn: "Id",
                keyValue: 2,
                column: "WebsiteUrl",
                value: "https://viendonghcm.edu.vn");
        }
    }
}
