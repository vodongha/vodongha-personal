using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace vodongha.Migrations
{
    /// <inheritdoc />
    public partial class UpdateExperienceDescriptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Experiences",
                keyColumn: "Id",
                keyValue: 1,
                column: "DescriptionEn",
                value: "Developing konfipay — an enterprise online banking platform implementing the EBICS standard. Full-stack development with ASP.NET Core, Blazor WASM, and SQL Server.");

            migrationBuilder.UpdateData(
                table: "Experiences",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Description", "DescriptionEn" },
                values: new object[] { "Phát triển Order — hệ thống quản lý đơn hàng nội bộ dành cho doanh nghiệp. Full-stack với Ruby on Rails 7, MySQL, Elasticsearch và Sidekiq.", "Developing Order — an internal order management system for enterprise use. Full-stack with Ruby on Rails 7, MySQL, Elasticsearch, and Sidekiq." });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Experiences",
                keyColumn: "Id",
                keyValue: 1,
                column: "DescriptionEn",
                value: null);

            migrationBuilder.UpdateData(
                table: "Experiences",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Description", "DescriptionEn" },
                values: new object[] { null, null });
        }
    }
}
