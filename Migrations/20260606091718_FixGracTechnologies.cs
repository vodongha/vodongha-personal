using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VodonghaPersonal.Migrations
{
    /// <inheritdoc />
    public partial class FixGracTechnologies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Projects",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Description", "DescriptionEn", "Technologies" },
                values: new object[] { "Hệ thống quản lý cho nền tảng đô thị không rác. Quản lý khách hàng, nhân viên, hợp đồng, tích hợp thanh toán MoMo và Payoo. Xây dựng bằng Laravel + MySQL.", "Management system for an eco-city waste platform. Handles customers, employees, contracts, with MoMo and Payoo payment gateway integration. Built with Laravel and MySQL.", "Laravel,PHP,JavaScript,jQuery,AJAX,MySQL,MoMo API,Payoo" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Projects",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Description", "DescriptionEn", "Technologies" },
                values: new object[] { "Hệ thống quản lý cho nền tảng đô thị không rác. Quản lý khách hàng, nhân viên, hợp đồng, thanh toán tích hợp MoMo Payment Gateway. Xây dựng bằng Laravel + PostgreSQL.", "Management system for an eco-city waste platform. Handles customers, employees, contracts, and payment with MoMo Payment Gateway integration. Built with Laravel and PostgreSQL.", "Laravel,PHP,JavaScript,jQuery,AJAX,PostgreSQL,MoMo API" });
        }
    }
}
