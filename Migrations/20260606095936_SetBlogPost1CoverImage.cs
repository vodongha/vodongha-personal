using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace vodongha.Migrations
{
    /// <inheritdoc />
    public partial class SetBlogPost1CoverImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE "BlogPosts"
                SET "CoverImageUrl" = 'https://i.ibb.co/9971LWDT/image.png'
                WHERE "Id" = 1 AND ("CoverImageUrl" IS NULL OR "CoverImageUrl" = '');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE "BlogPosts" SET "CoverImageUrl" = NULL WHERE "Id" = 1;
                """);
        }
    }
}
