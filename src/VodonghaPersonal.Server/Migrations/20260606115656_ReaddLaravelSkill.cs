using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VodonghaPersonal.Migrations
{
    /// <inheritdoc />
    public partial class ReaddLaravelSkill : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                INSERT INTO "Skills" ("Id", "Name", "Category", "Icon", "Proficiency", "Order")
                SELECT 3, 'Ruby on Rails', 'Backend', 'devicon-rails-plain', 80, 3
                WHERE NOT EXISTS (SELECT 1 FROM "Skills" WHERE "Id" = 3);
                """);
            migrationBuilder.Sql("""
                INSERT INTO "Skills" ("Id", "Name", "Category", "Icon", "Proficiency", "Order")
                SELECT 4, 'Laravel', 'Backend', 'devicon-laravel-plain', 75, 4
                WHERE NOT EXISTS (SELECT 1 FROM "Skills" WHERE "Id" = 4);
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""DELETE FROM "Skills" WHERE "Id" IN (3, 4) AND "Name" IN ('Ruby on Rails', 'Laravel');""");
        }
    }
}
