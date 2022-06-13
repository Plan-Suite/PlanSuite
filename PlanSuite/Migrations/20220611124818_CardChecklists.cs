using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlanSuite.Migrations
{
    public partial class CardChecklists : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "card_checklists",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    checklist_name = table.Column<string>(type: "longtext", nullable: false, collation: "latin1_swedish_ci"),
                    checklist_card = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_card_checklists", x => x.id);
                })
                .Annotation("Relational:Collation", "latin1_swedish_ci");

            migrationBuilder.CreateTable(
                name: "checklist_items",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    checklist_id = table.Column<int>(type: "int", nullable: false),
                    item_name = table.Column<string>(type: "longtext", nullable: false, collation: "latin1_swedish_ci"),
                    item_ticked = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    item_index = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_checklist_items", x => x.id);
                })
                .Annotation("Relational:Collation", "latin1_swedish_ci");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "card_checklists");

            migrationBuilder.DropTable(
                name: "checklist_items");
        }
    }
}
