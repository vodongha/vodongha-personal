using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace vodongha.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSkillsAndProjects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Projects",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "Description", "GitHubUrl", "LiveUrl", "Technologies", "Title" },
                values: new object[] { new DateTime(2021, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Nền tảng ngân hàng trực tuyến doanh nghiệp hỗ trợ chuẩn EBICS. Quản lý hàng trăm tài khoản ngân hàng, tích hợp REST API cho ERP, multi-tenant, tự động hoá thanh toán.", null, "https://konfipay.de", "C#,ASP.NET Core,Blazor,SQL Server,Hangfire,Azure", "konfipay" });

            migrationBuilder.InsertData(
                table: "Projects",
                columns: new[] { "Id", "CreatedAt", "Description", "GitHubUrl", "ImageUrl", "IsFeatured", "LiveUrl", "Order", "Technologies", "Title" },
                values: new object[] { 2, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Web cá nhân xây dựng với Blazor Web App .NET 10 và PostgreSQL. Deploy tự động lên Fly.io qua GitHub Actions, SCSS dark theme.", "https://github.com/vodongha/vodongha-personal", null, true, "https://vodongha.id.vn", 2, "Blazor,.NET 10,PostgreSQL,SCSS,Fly.io,Docker", "Personal Website" });

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 2,
                column: "Proficiency",
                value: 88);

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Category", "Icon", "Name" },
                values: new object[] { "Backend", "devicon-rails-plain", "Ruby on Rails" });

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Category", "Icon", "Name" },
                values: new object[] { "Backend", "devicon-laravel-plain", "Laravel" });

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Category", "Icon", "Name", "Proficiency" },
                values: new object[] { "Frontend", "devicon-blazor-plain", "Blazor", 85 });

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Category", "Icon", "Name", "Proficiency" },
                values: new object[] { "Frontend", "devicon-javascript-plain", "JavaScript", 75 });

            migrationBuilder.InsertData(
                table: "Skills",
                columns: new[] { "Id", "Category", "Icon", "Name", "Order", "Proficiency" },
                values: new object[,]
                {
                    { 7, "Frontend", "devicon-html5-plain", "HTML / CSS", 7, 85 },
                    { 8, "Database", "devicon-postgresql-plain", "PostgreSQL", 8, 80 },
                    { 9, "Database", "devicon-mysql-plain", "MySQL", 9, 75 },
                    { 10, "Database", "devicon-microsoftsqlserver-plain", "SQL Server", 10, 80 },
                    { 11, "DevOps", "devicon-docker-plain", "Docker", 11, 70 },
                    { 12, "DevOps", "devicon-git-plain", "Git", 12, 85 },
                    { 13, "DevOps", "devicon-azure-plain", "Azure", 13, 65 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Projects",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.UpdateData(
                table: "Projects",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "Description", "GitHubUrl", "LiveUrl", "Technologies", "Title" },
                values: new object[] { new DateTime(2026, 6, 6, 0, 0, 0, 0, DateTimeKind.Utc), "Web cá nhân được xây dựng với Blazor Web App .NET 10 và PostgreSQL.", "https://github.com/vodongha/vodongha.id.vn", "https://vodongha.id.vn", "Blazor,.NET 10,PostgreSQL,SCSS", "Personal Website" });

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 2,
                column: "Proficiency",
                value: 85);

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Category", "Icon", "Name" },
                values: new object[] { "Frontend", "devicon-blazor-plain", "Blazor" });

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Category", "Icon", "Name" },
                values: new object[] { "Database", "devicon-postgresql-plain", "PostgreSQL" });

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Category", "Icon", "Name", "Proficiency" },
                values: new object[] { "DevOps", "devicon-docker-plain", "Docker", 70 });

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Category", "Icon", "Name", "Proficiency" },
                values: new object[] { "DevOps", "devicon-git-plain", "Git", 85 });
        }
    }
}
