using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VodonghaPersonal.Migrations
{
    /// <inheritdoc />
    public partial class AddRidToAllEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            string[] tables = ["AppSecrets", "BlogPosts", "ChatMessages", "ChatSessions",
                "ContactMessages", "Educations", "Experiences", "Projects", "Skills"];

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            string[] tables = ["AppSecrets", "BlogPosts", "ChatMessages", "ChatSessions",
                "ContactMessages", "Educations", "Experiences", "Projects", "Skills"];

            foreach (string table in tables)
            {
                migrationBuilder.DropColumn(name: "Rid", table: table);
            }
        }
    }
}
