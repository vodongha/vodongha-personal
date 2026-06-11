using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VodonghaPersonal.Migrations
{
    [Migration("20260611140000_AddRidToRemainingTables")]
    public partial class AddRidToRemainingTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            string[] tables = ["AdminUsers", "PageViews", "PushSubscriptions", "SiteSettings", "VisitorLogs"];

            foreach (string table in tables)
            {
                migrationBuilder.AddColumn<Guid>(
                    name: "Rid",
                    table: table,
                    type: "uuid",
                    nullable: false,
                    defaultValueSql: "gen_random_uuid()");
            }
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            string[] tables = ["AdminUsers", "PageViews", "PushSubscriptions", "SiteSettings", "VisitorLogs"];

            foreach (string table in tables)
            {
                migrationBuilder.DropColumn(name: "Rid", table: table);
            }
        }
    }
}
