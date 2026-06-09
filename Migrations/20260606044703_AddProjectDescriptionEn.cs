using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VodonghaPersonal.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectDescriptionEn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DescriptionEn",
                table: "Projects",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Projects",
                keyColumn: "Id",
                keyValue: 1,
                column: "DescriptionEn",
                value: "Enterprise-scale online banking platform supporting the EBICS standard. Manages hundreds of bank accounts across 13+ banks with REST API for ERP integration, multi-tenancy, and automated payment signing. Integrates PayPal and Atlassian/Jira. Deployed on Azure (SaaS) and Swisscom Docker (konfipay.ch) for Swiss customers requiring data sovereignty.");

            migrationBuilder.UpdateData(
                table: "Projects",
                keyColumn: "Id",
                keyValue: 2,
                column: "DescriptionEn",
                value: "Personal website built with Blazor Web App .NET 10 and PostgreSQL. Auto-deployed to Fly.io via GitHub Actions with SCSS dark theme.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DescriptionEn",
                table: "Projects");
        }
    }
}
