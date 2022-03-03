using Microsoft.EntityFrameworkCore.Migrations;

namespace Assignment4WC.Context.Migrations
{
    public partial class AddedLocationHint : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LocationHint",
                table: "Questions",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LocationHint",
                table: "Questions");
        }
    }
}
