using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VodonghaPersonal.Migrations
{
    /// <inheritdoc />
    public partial class UpdateKonfipayProjectDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Projects",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Description", "DescriptionEn", "Technologies" },
                values: new object[] { "Nền tảng ngân hàng trực tuyến doanh nghiệp quy mô lớn theo chuẩn EBICS. Quản lý hàng trăm tài khoản ngân hàng trên 13+ ngân hàng, cung cấp REST API cho tích hợp ERP/tự động hoá, kiến trúc multi-tenant với database riêng mỗi khách hàng, tự động ký và nộp lệnh thanh toán SEPA, xử lý file camt/MT940/pain. Tích hợp PayPal, Atlassian/Jira. Hỗ trợ FinTS/XS2A (Open Banking). Deploy trên Azure (SaaS) và Swisscom Docker (konfipay.ch) cho khách hàng Thụy Sĩ yêu cầu data sovereignty.", "Enterprise-scale online banking platform implementing the EBICS standard. Manages hundreds of bank accounts across 13+ banks with a REST API for ERP integration and automation, per-tenant SQL Server database architecture, automated SEPA payment signing and submission, and camt/MT940/pain file processing. Integrates PayPal and Atlassian/Jira. Supports FinTS/XS2A (Open Banking). Deployed on Azure (SaaS) and Swisscom Docker (konfipay.ch) for Swiss customers requiring data sovereignty.", "C#,.NET 9,ASP.NET Core,Blazor WASM,SQL Server,Hangfire,Serilog,EBICS,FinTS/XS2A,Azure,Docker,PayPal API,Jira API" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Projects",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Description", "DescriptionEn", "Technologies" },
                values: new object[] { "Nền tảng ngân hàng trực tuyến doanh nghiệp quy mô lớn, hỗ trợ chuẩn EBICS (European Banking Standard). Quản lý hàng trăm tài khoản ngân hàng trên 13+ ngân hàng, REST API cho tích hợp ERP, multi-tenant, tự động hoá ký và nộp lệnh thanh toán. Tích hợp PayPal, Atlassian/Jira. Deploy trên Azure (SaaS) và Swisscom Docker (konfipay.ch) cho khách hàng Thụy Sĩ yêu cầu data sovereignty.", "Enterprise-scale online banking platform supporting the EBICS standard. Manages hundreds of bank accounts across 13+ banks with REST API for ERP integration, multi-tenancy, and automated payment signing. Integrates PayPal and Atlassian/Jira. Deployed on Azure (SaaS) and Swisscom Docker (konfipay.ch) for Swiss customers requiring data sovereignty.", "C#,.NET 9,ASP.NET Core,Blazor WASM,SQL Server,Hangfire,Serilog,Azure,Docker,EBICS" });
        }
    }
}
