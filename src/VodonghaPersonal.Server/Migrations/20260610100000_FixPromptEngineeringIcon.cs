using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VodonghaPersonal.Migrations
{
    [Migration("20260610100000_FixPromptEngineeringIcon")]
    public partial class FixPromptEngineeringIcon : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // bi-chat-dots not rendering correctly — use bi-lightbulb (semantically correct for prompt engineering)
            migrationBuilder.Sql("UPDATE \"Skills\" SET \"Icon\" = 'bi bi-lightbulb' WHERE \"Name\" = 'Prompt Engineering'");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE \"Skills\" SET \"Icon\" = 'bi bi-chat-dots' WHERE \"Name\" = 'Prompt Engineering'");
        }
    }
}
