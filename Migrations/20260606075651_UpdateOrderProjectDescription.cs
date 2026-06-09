using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VodonghaPersonal.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOrderProjectDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Projects",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Description", "DescriptionEn", "Technologies" },
                values: new object[] { "Hệ thống quản lý đơn hàng nội bộ dành cho doanh nghiệp. Xây dựng với Ruby on Rails 7, tích hợp Elasticsearch để tìm kiếm và lọc đơn hàng theo thời gian thực. Hỗ trợ xuất báo cáo PDF/Excel, xử lý background job với Sidekiq, phân quyền người dùng với Devise + Pundit, và kết nối SFTP để đồng bộ dữ liệu.", "Internal order management system for enterprise use. Built with Ruby on Rails 7, featuring real-time search and filtering via Elasticsearch, PDF/Excel report export, background job processing with Sidekiq, role-based access control with Devise and Pundit, and SFTP integration for data synchronisation.", "Ruby on Rails 7,MySQL,Elasticsearch,Sidekiq,jQuery,SCSS,Docker,SFTP,PDF/Excel export" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Projects",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Description", "DescriptionEn", "Technologies" },
                values: new object[] { "Hệ thống quản lý đơn hàng full-stack. Xây dựng với Ruby on Rails, CoffeeScript và tích hợp Elasticsearch để tìm kiếm và lọc đơn hàng nhanh chóng.", "Full-stack order management system built with Ruby on Rails and CoffeeScript, integrated with Elasticsearch for fast order search and filtering.", "Ruby on Rails,CoffeeScript,Elasticsearch,jQuery,PostgreSQL" });
        }
    }
}
