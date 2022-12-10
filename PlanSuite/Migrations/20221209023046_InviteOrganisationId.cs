using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlanSuite.Migrations
{
    public partial class InviteOrganisationId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "invite_organisation",
                table: "invitations",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "invite_organisation",
                table: "invitations");
        }
    }
}
