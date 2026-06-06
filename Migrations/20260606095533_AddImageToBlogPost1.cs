using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace vodongha.Migrations
{
    /// <inheritdoc />
    public partial class AddImageToBlogPost1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE "BlogPosts"
                SET "Content" = REPLACE(
                    "Content",
                    '<h2>Bắt đầu từ đâu?</h2>',
                    '<figure style="margin:1.5rem 0"><img src="https://i.ibb.co/9971LWDT/image.png" alt="Claude Code in action" style="width:100%;border-radius:10px" /></figure><h2>Bắt đầu từ đâu?</h2>'
                )
                WHERE "Id" = 1 AND "Content" NOT LIKE '%i.ibb.co/9971LWDT%';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE "BlogPosts"
                SET "Content" = REPLACE(
                    "Content",
                    '<figure style="margin:1.5rem 0"><img src="https://i.ibb.co/9971LWDT/image.png" alt="Claude Code in action" style="width:100%;border-radius:10px" /></figure>',
                    ''
                )
                WHERE "Id" = 1;
                """);
        }
    }
}
