using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VodonghaPersonal.Migrations
{
    /// <inheritdoc />
    public partial class FixFotoSolutionTechStack : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Projects",
                keyColumn: "Id",
                keyValue: 4,
                column: "Technologies",
                value: "ASP.NET Core 3.1,C#,JavaScript,jQuery,CSS,Dropbox API,FTP,SQL Server");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Projects",
                keyColumn: "Id",
                keyValue: 4,
                column: "Technologies",
                value: "Ruby on Rails,JavaScript,jQuery,CoffeeScript,CSS,Dropbox API,FTP");
        }
    }
}
