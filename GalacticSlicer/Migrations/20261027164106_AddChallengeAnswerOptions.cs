using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GalacticSlicer.Migrations
{
    /// <inheritdoc />
    public partial class AddChallengeAnswerOptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OptionA",
                table: "Challenges",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OptionB",
                table: "Challenges",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OptionC",
                table: "Challenges",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OptionD",
                table: "Challenges",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OptionA",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "OptionB",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "OptionC",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "OptionD",
                table: "Challenges");
        }
    }
}
