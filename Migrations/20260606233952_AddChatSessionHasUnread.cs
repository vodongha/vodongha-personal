using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace vodongha.Migrations
{
    /// <inheritdoc />
    public partial class AddChatSessionHasUnread : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasUnread",
                table: "ChatSessions",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasUnread",
                table: "ChatSessions");
        }
    }
}
