using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VodonghaPersonal.Migrations
{
    [Migration("20260609150000_FixSkillIcons2")]
    public partial class FixSkillIcons2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE \"Skills\" SET \"Icon\" = 'bi bi-filetype-json' WHERE \"Name\" = 'JSON'");
            migrationBuilder.Sql("UPDATE \"Skills\" SET \"Icon\" = 'devicon-coffeescript-original' WHERE \"Name\" = 'CoffeeScript'");
            migrationBuilder.Sql("UPDATE \"Skills\" SET \"Icon\" = 'bi bi-airplane-fill' WHERE \"Name\" = 'Fly.io'");
            migrationBuilder.Sql("UPDATE \"Skills\" SET \"Icon\" = 'bi bi-broadcast' WHERE \"Name\" = 'SignalR'");
            migrationBuilder.Sql("UPDATE \"Skills\" SET \"Icon\" = 'bi bi-clock-history' WHERE \"Name\" = 'Hangfire'");
            migrationBuilder.Sql("UPDATE \"Skills\" SET \"Icon\" = 'bi bi-chat-dots' WHERE \"Name\" = 'Prompt Engineering'");
            migrationBuilder.Sql("UPDATE \"Skills\" SET \"Icon\" = 'bi bi-robot' WHERE \"Name\" = 'AI-Assisted Dev'");
            migrationBuilder.Sql("UPDATE \"Skills\" SET \"Icon\" = 'devicon-sass-original' WHERE \"Name\" = 'SCSS / Sass'");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE \"Skills\" SET \"Icon\" = 'devicon-json-plain' WHERE \"Name\" = 'JSON'");
            migrationBuilder.Sql("UPDATE \"Skills\" SET \"Icon\" = 'devicon-coffeescript-plain' WHERE \"Name\" = 'CoffeeScript'");
            migrationBuilder.Sql("UPDATE \"Skills\" SET \"Icon\" = 'devicon-flyio-plain' WHERE \"Name\" = 'Fly.io'");
            migrationBuilder.Sql("UPDATE \"Skills\" SET \"Icon\" = 'devicon-dotnetcore-plain' WHERE \"Name\" = 'SignalR'");
            migrationBuilder.Sql("UPDATE \"Skills\" SET \"Icon\" = 'devicon-dotnetcore-plain' WHERE \"Name\" = 'Hangfire'");
            migrationBuilder.Sql("UPDATE \"Skills\" SET \"Icon\" = 'devicon-tensorflow-plain' WHERE \"Name\" = 'Prompt Engineering'");
            migrationBuilder.Sql("UPDATE \"Skills\" SET \"Icon\" = 'devicon-vscode-plain' WHERE \"Name\" = 'AI-Assisted Dev'");
            migrationBuilder.Sql("UPDATE \"Skills\" SET \"Icon\" = 'devicon-sass-plain' WHERE \"Name\" = 'SCSS / Sass'");
        }
    }
}
