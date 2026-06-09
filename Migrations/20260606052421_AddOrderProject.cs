using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VodonghaPersonal.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderProject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Projects",
                columns: new[] { "Id", "CreatedAt", "Description", "DescriptionEn", "GitHubUrl", "ImageUrl", "IsFeatured", "LiveUrl", "Order", "Technologies", "Title" },
                values: new object[] { 5, new DateTime(2021, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Hệ thống quản lý đơn hàng full-stack. Xây dựng với Ruby on Rails, CoffeeScript và tích hợp Elasticsearch để tìm kiếm và lọc đơn hàng nhanh chóng.", "Full-stack order management system built with Ruby on Rails and CoffeeScript, integrated with Elasticsearch for fast order search and filtering.", null, null, false, null, 5, "Ruby on Rails,CoffeeScript,Elasticsearch,jQuery,PostgreSQL", "Order" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Projects",
                keyColumn: "Id",
                keyValue: 5);
        }
    }
}
