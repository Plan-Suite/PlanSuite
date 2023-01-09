using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlanSuite.Migrations
{
    public partial class Blog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "blog_posts",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    author_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    date_posted = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    date_modified = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    category_id = table.Column<int>(type: "int", nullable: true),
                    title = table.Column<string>(type: "longtext", nullable: false, collation: "latin1_swedish_ci"),
                    summary = table.Column<string>(type: "longtext", nullable: false, collation: "latin1_swedish_ci"),
                    content = table.Column<string>(type: "longtext", nullable: false, collation: "latin1_swedish_ci"),
                    slug = table.Column<string>(type: "varchar(255)", nullable: false, collation: "latin1_swedish_ci"),
                    comments_disabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    image = table.Column<string>(type: "longtext", nullable: true, collation: "latin1_swedish_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_blog_posts", x => x.id);
                })
                .Annotation("Relational:Collation", "latin1_swedish_ci");

            migrationBuilder.CreateIndex(
                name: "IX_blog_posts_slug",
                table: "blog_posts",
                column: "slug",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "blog_posts");
        }
    }
}
