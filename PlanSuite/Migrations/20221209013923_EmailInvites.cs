using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlanSuite.Migrations
{
    public partial class EmailInvites : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "invitations",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    invite_code = table.Column<string>(type: "longtext", nullable: false, collation: "latin1_swedish_ci"),
                    invite_accepted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    invite_expiry = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    invite_email = table.Column<string>(type: "longtext", nullable: false, collation: "latin1_swedish_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invitations", x => x.id);
                })
                .Annotation("Relational:Collation", "latin1_swedish_ci");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "invitations");
        }
    }
}
