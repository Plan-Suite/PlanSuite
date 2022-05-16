using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlanSuite.Migrations
{
    public partial class changelogs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "change_log",
                columns: table => new
                {
                    id = table.Column<string>(type: "varchar(255)", nullable: false, collation: "latin1_swedish_ci"),
                    version_name = table.Column<string>(type: "longtext", nullable: false, collation: "latin1_swedish_ci"),
                    additions = table.Column<string>(type: "longtext", nullable: false, collation: "latin1_swedish_ci"),
                    changes = table.Column<string>(type: "longtext", nullable: false, collation: "latin1_swedish_ci"),
                    fixes = table.Column<string>(type: "longtext", nullable: false, collation: "latin1_swedish_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_change_log", x => x.id);
                })
                .Annotation("Relational:Collation", "latin1_swedish_ci");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "change_log");
        }
    }
}
