using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace VodonghaPersonal.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingExperiences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Experiences",
                columns: new[] { "Id", "Company", "Description", "DescriptionEn", "EndMonth", "EndYear", "IsCurrent", "Location", "Order", "Role", "StartMonth", "StartYear", "WebsiteUrl" },
                values: new object[,]
                {
                    { 3, "Trung tâm dạy nghề lái xe Sài Gòn", "Huấn luyện viên dạy lái xe ô tô.", "Car driving instructor.", 12, 2022, false, "Dong Nai", 3, "Driving Instructor", 5, 2020, null },
                    { 4, "Be Group", "Tài xế xe hơi công nghệ.", "Ride-hailing car driver.", 10, 2020, false, "Ho Chi Minh City", 4, "Car Driver", 3, 2020, null },
                    { 5, "Gojek", "Tài xế xe máy công nghệ.", "Ride-hailing motorbike driver.", 3, 2020, false, "Ho Chi Minh City", 5, "Motorbike Driver", 4, 2018, null },
                    { 6, "Uber", "Tài xế xe máy công nghệ.", "Ride-hailing motorbike driver.", 4, 2018, false, "Ho Chi Minh City", 6, "Motorbike Driver", 8, 2016, null },
                    { 7, "Bến Thành Ford", "Thực tập kỹ thuật viên ô tô.", "Automotive technician internship.", 4, 2016, false, "Ho Chi Minh City", 7, "Automotive Technician Internship", 2, 2016, null },
                    { 8, "Grab", "Tài xế xe máy công nghệ.", "Ride-hailing motorbike driver.", 1, 2016, false, "Ho Chi Minh City", 8, "Motorbike Driver", 5, 2015, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Experiences",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Experiences",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Experiences",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Experiences",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Experiences",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Experiences",
                keyColumn: "Id",
                keyValue: 8);
        }
    }
}
