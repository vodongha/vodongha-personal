using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace VodonghaPersonal.Migrations
{
    /// <inheritdoc />
    public partial class AddLinkedInData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Projects",
                columns: new[] { "Id", "CreatedAt", "Description", "DescriptionEn", "GitHubUrl", "ImageUrl", "IsFeatured", "LiveUrl", "Order", "Technologies", "Title" },
                values: new object[,]
                {
                    { 3, new DateTime(2021, 7, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Hệ thống quản lý cho nền tảng đô thị không rác. Quản lý khách hàng, nhân viên, hợp đồng, thanh toán tích hợp MoMo Payment Gateway.", "Management system for an eco-city waste platform. Handles customers, employees, contracts, and payment with MoMo Payment Gateway integration.", null, null, false, "https://e.grac.vn", 3, "Ruby on Rails,JavaScript,jQuery,AJAX,PostgreSQL,MoMo API", "Grac" },
                    { 4, new DateTime(2020, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Website quản lý studio ảnh: khách hàng, sale, editor, admin. Khách hàng upload ảnh qua Dropbox/FTP, editor chỉnh sửa và ghi chú, quản lý doanh thu và thanh toán.", "Photo studio management platform: client portal, sales, editor workflow, and admin dashboard. Clients upload images via Dropbox/FTP, editors annotate and design, with sales reporting and payment management.", null, null, false, null, 4, "Ruby on Rails,JavaScript,jQuery,CoffeeScript,CSS,Dropbox API,FTP", "Foto Solution" }
                });

            migrationBuilder.InsertData(
                table: "Skills",
                columns: new[] { "Id", "Category", "Icon", "Name", "Order", "Proficiency" },
                values: new object[,]
                {
                    { 18, "Frontend", "devicon-jquery-plain", "jQuery", 18, 75 },
                    { 19, "Frontend", "devicon-javascript-plain", "AJAX", 19, 72 },
                    { 20, "Backend", "devicon-json-plain", "JSON", 20, 85 },
                    { 21, "Frontend", "devicon-coffeescript-plain", "CoffeeScript", 21, 60 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Projects",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Projects",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 21);
        }
    }
}
