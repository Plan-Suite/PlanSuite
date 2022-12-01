using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlanSuite.Migrations
{
    public partial class SalesContactTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "sales_contacts",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    first_name = table.Column<string>(type: "longtext", nullable: false, collation: "latin1_general_ci"),
                    last_name = table.Column<string>(type: "longtext", nullable: false, collation: "latin1_general_ci"),
                    email = table.Column<string>(type: "longtext", nullable: false, collation: "latin1_general_ci"),
                    job_title = table.Column<string>(type: "longtext", nullable: false, collation: "latin1_general_ci"),
                    phone_number = table.Column<string>(type: "longtext", nullable: false, collation: "latin1_general_ci"),
                    message = table.Column<string>(type: "longtext", nullable: false, collation: "latin1_general_ci"),
                    timestamp = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    is_contacted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sales_contacts", x => x.id);
                })
                .Annotation("Relational:Collation", "latin1_general_ci");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "sales_contacts");
        }
    }
}
