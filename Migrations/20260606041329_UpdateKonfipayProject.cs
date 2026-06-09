using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VodonghaPersonal.Migrations
{
    /// <inheritdoc />
    public partial class UpdateKonfipayProject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Projects",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Description", "LiveUrl", "Technologies" },
                values: new object[] { "Nền tảng ngân hàng trực tuyến doanh nghiệp quy mô lớn, hỗ trợ chuẩn EBICS (European Banking Standard). Quản lý hàng trăm tài khoản ngân hàng trên 13+ ngân hàng, REST API cho tích hợp ERP, multi-tenant, tự động hoá ký và nộp lệnh thanh toán. Tích hợp PayPal, Atlassian/Jira. Deploy trên Azure (SaaS) và Swisscom Docker (konfipay.ch) cho khách hàng Thụy Sĩ yêu cầu data sovereignty.", "https://portal.konfipay.de", "C#,.NET 9,ASP.NET Core,Blazor WASM,SQL Server,Hangfire,Serilog,Azure,Docker,EBICS" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Projects",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Description", "LiveUrl", "Technologies" },
                values: new object[] { "Nền tảng ngân hàng trực tuyến doanh nghiệp hỗ trợ chuẩn EBICS. Quản lý hàng trăm tài khoản ngân hàng, tích hợp REST API cho ERP, multi-tenant, tự động hoá thanh toán.", "https://konfipay.de", "C#,ASP.NET Core,Blazor,SQL Server,Hangfire,Azure" });
        }
    }
}
