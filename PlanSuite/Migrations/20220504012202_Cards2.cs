using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlanSuite.Migrations
{
    public partial class Cards2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "card_description",
                table: "cards",
                type: "longtext",
                nullable: false,
                collation: "latin1_swedish_ci");

            migrationBuilder.AddColumn<string>(
                name: "card_name",
                table: "cards",
                type: "longtext",
                nullable: false,
                collation: "latin1_swedish_ci");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "card_description",
                table: "cards");

            migrationBuilder.DropColumn(
                name: "card_name",
                table: "cards");
        }
    }
}
