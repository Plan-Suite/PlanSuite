using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlanSuite.Migrations
{
    public partial class Milestones : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "card_milestone",
                table: "cards",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "card_milestone",
                table: "cards");
        }
    }
}
