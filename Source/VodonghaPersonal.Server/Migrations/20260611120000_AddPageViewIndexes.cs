using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VodonghaPersonal.Migrations
{
    [Migration("20260611120000_AddPageViewIndexes")]
    public partial class AddPageViewIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_PageViews_Path",
                table: "PageViews",
                column: "Path");

            migrationBuilder.CreateIndex(
                name: "IX_PageViews_Country",
                table: "PageViews",
                column: "Country");

            migrationBuilder.CreateIndex(
                name: "IX_PageViews_Referrer",
                table: "PageViews",
                column: "Referrer");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_PageViews_Path", table: "PageViews");
            migrationBuilder.DropIndex(name: "IX_PageViews_Country", table: "PageViews");
            migrationBuilder.DropIndex(name: "IX_PageViews_Referrer", table: "PageViews");
        }
    }
}
