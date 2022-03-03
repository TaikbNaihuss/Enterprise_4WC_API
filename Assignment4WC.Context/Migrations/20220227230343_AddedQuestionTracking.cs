using Microsoft.EntityFrameworkCore.Migrations;

namespace Assignment4WC.Context.Migrations
{
    public partial class AddedQuestionTracking : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.AddColumn<int>(
                name: "CurrentQuestionNumber",
                table: "Members",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "QuestionIds",
                table: "Members",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropColumn(
                name: "CurrentQuestionNumber",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "QuestionIds",
                table: "Members");
        }
    }
}
