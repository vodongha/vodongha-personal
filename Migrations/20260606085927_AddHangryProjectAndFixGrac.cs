using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VodonghaPersonal.Migrations
{
    /// <inheritdoc />
    public partial class AddHangryProjectAndFixGrac : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Projects",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Description", "DescriptionEn", "Technologies" },
                values: new object[] { "Hệ thống quản lý cho nền tảng đô thị không rác. Quản lý khách hàng, nhân viên, hợp đồng, thanh toán tích hợp MoMo Payment Gateway. Xây dựng bằng Laravel + PostgreSQL.", "Management system for an eco-city waste platform. Handles customers, employees, contracts, and payment with MoMo Payment Gateway integration. Built with Laravel and PostgreSQL.", "Laravel,PHP,JavaScript,jQuery,AJAX,PostgreSQL,MoMo API" });

            migrationBuilder.InsertData(
                table: "Projects",
                columns: new[] { "Id", "CreatedAt", "Description", "DescriptionEn", "GitHubUrl", "ImageUrl", "IsFeatured", "LiveUrl", "Order", "Technologies", "Title" },
                values: new object[] { 6, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Hệ thống đặt đồ ăn phục vụ phòng khách sạn. Khách xác thực bằng số phòng + PIN hoặc quét mã QR, xem thực đơn tuần và đặt order tính vào hóa đơn phòng. Theo dõi đơn hàng real-time qua SignalR. Kitchen staff quản lý orders và thực đơn theo thời gian thực từ dashboard riêng.", "Hotel room service ordering system. Guests authenticate via room number + PIN or QR code scan, browse the weekly menu, and place orders charged directly to their room bill. Real-time order tracking via SignalR. Kitchen staff manage live orders and menu from a dedicated dashboard.", null, null, true, null, 6, "Blazor WASM,.NET 10,ASP.NET Core,SignalR,PostgreSQL,EF Core,.NET Aspire,SCSS,JWT,Bootstrap 5", "Hangry" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Projects",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.UpdateData(
                table: "Projects",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Description", "DescriptionEn", "Technologies" },
                values: new object[] { "Hệ thống quản lý cho nền tảng đô thị không rác. Quản lý khách hàng, nhân viên, hợp đồng, thanh toán tích hợp MoMo Payment Gateway.", "Management system for an eco-city waste platform. Handles customers, employees, contracts, and payment with MoMo Payment Gateway integration.", "Ruby on Rails,JavaScript,jQuery,AJAX,PostgreSQL,MoMo API" });
        }
    }
}
