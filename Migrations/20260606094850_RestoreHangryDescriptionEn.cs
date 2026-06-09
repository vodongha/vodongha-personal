using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VodonghaPersonal.Migrations
{
    /// <inheritdoc />
    public partial class RestoreHangryDescriptionEn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "UPDATE \"Projects\" SET \"DescriptionEn\" = 'Hotel room service ordering system. Guests authenticate via room number + PIN or QR code scan, browse the weekly menu, and place orders charged directly to their room bill. Real-time order tracking via SignalR. Kitchen staff manage live orders and menu from a dedicated dashboard.' WHERE \"Id\" = 6 AND (\"DescriptionEn\" IS NULL OR \"DescriptionEn\" = '');"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
