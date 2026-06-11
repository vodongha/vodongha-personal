using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace VodonghaPersonal.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminPanel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Educations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    School = table.Column<string>(type: "text", nullable: false),
                    Degree = table.Column<string>(type: "text", nullable: false),
                    Field = table.Column<string>(type: "text", nullable: false),
                    StartYear = table.Column<int>(type: "integer", nullable: false),
                    EndYear = table.Column<int>(type: "integer", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Educations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Experiences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Company = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false),
                    Location = table.Column<string>(type: "text", nullable: true),
                    StartYear = table.Column<int>(type: "integer", nullable: false),
                    StartMonth = table.Column<int>(type: "integer", nullable: false),
                    EndYear = table.Column<int>(type: "integer", nullable: true),
                    EndMonth = table.Column<int>(type: "integer", nullable: true),
                    IsCurrent = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Experiences", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Educations",
                columns: new[] { "Id", "Degree", "Description", "EndYear", "Field", "Order", "School", "StartYear" },
                values: new object[,]
                {
                    { 1, "Bachelor's degree", null, 2020, "Computer Software Engineering", 1, "Nguyen Tat Thanh University", 2016 },
                    { 2, "Associate's degree", null, 2016, "Automotive Engineering Technology", 2, "Vien Dong College", 2013 }
                });

            migrationBuilder.InsertData(
                table: "Experiences",
                columns: new[] { "Id", "Company", "Description", "EndMonth", "EndYear", "IsCurrent", "Location", "Order", "Role", "StartMonth", "StartYear" },
                values: new object[,]
                {
                    { 1, "cisbox GmbH", "Phát triển konfipay — nền tảng ngân hàng trực tuyến doanh nghiệp chuẩn EBICS. Full-stack với ASP.NET Core, Blazor WASM, SQL Server.", null, null, true, "Ho Chi Minh City", 1, "Web Developer (Onsite)", 10, 2021 },
                    { 2, "BSP Software Services Corporation", null, null, null, true, "Ho Chi Minh City", 2, "Web Developer", 10, 2020 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Educations");

            migrationBuilder.DropTable(
                name: "Experiences");
        }
    }
}
