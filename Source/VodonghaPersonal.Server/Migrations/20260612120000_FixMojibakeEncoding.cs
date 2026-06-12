using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VodonghaPersonal.Migrations
{
    [Migration("20260612120000_FixMojibakeEncoding")]
    public partial class FixMojibakeEncoding : Migration
    {
        // Mojibake: UTF-8 bytes of special chars were decoded as Windows-1252, stored, then re-encoded.
        // Each pair: (mojibake string in DB, correct Unicode replacement).
        // All special chars expressed as \u escapes to avoid source-file encoding ambiguity.
        private static readonly (string Bad, string Good)[] Replacements =
        [
            ("Â·",                   "·"), // Â·  → · (middle dot U+00B7)
            ("â€”",             "—"), // â€" → — (em-dash U+2014)
            ("â€“",             "–"), // â€" → – (en-dash U+2013)
            ("â€™",             "’"), // â€™ → ' (right single quote U+2019)
            ("â€˜",             "‘"), // â€˜ → ' (left single quote U+2018)
            ("â€œ",             "“"), // â€œ → " (left double quote U+201C)
            ("â€",             "”"), // â€  → " (right double quote U+201D)
            ("â€¦",             "…"), // â€¦ → … (ellipsis U+2026)
            ("Ã©",                   "é"), // Ã©  → é
            ("Ã ",                   "à"), // Ã    → à
            ("Ã¨",                   "è"), // Ã¨  → è
        ];

        private static readonly (string Table, string[] Columns)[] Targets =
        [
            ("Experiences", ["Company", "Role", "Location", "Description", "DescriptionEn", "WebsiteUrl"]),
            ("Educations",  ["School", "Degree", "Field", "Description", "DescriptionEn", "WebsiteUrl"]),
            ("Projects",    ["Title", "TitleEn", "Description", "DescriptionEn"]),
            ("Skills",      ["Name"]),
            ("SiteSettings", ["Value"]),
            ("BlogPosts",   ["Title", "Summary", "SummaryEn", "Content", "ContentEn"]),
        ];

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            foreach ((string table, string[] columns) in Targets)
            {
                foreach (string column in columns)
                {
                    string expr = $"\"{column}\"";
                    foreach ((string bad, string good) in Replacements)
                    {
                        expr = $"REPLACE({expr}, '{bad}', '{good}')";
                    }

                    migrationBuilder.Sql($"UPDATE \"{table}\" SET \"{column}\" = {expr} WHERE \"{column}\" IS NOT NULL;");
                }
            }
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Encoding fixes are not reversible.
        }
    }
}
