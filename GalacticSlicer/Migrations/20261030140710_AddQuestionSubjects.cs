using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GalacticSlicer.Migrations
{
    /// <inheritdoc />
    public partial class AddQuestionSubjects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Subject",
                table: "Planets",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Explanation",
                table: "Challenges",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceId",
                table: "Challenges",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Subject",
                table: "Challenges",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Challenges_SourceId",
                table: "Challenges",
                column: "SourceId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Challenges_SourceId",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "Subject",
                table: "Planets");

            migrationBuilder.DropColumn(
                name: "Explanation",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "SourceId",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "Subject",
                table: "Challenges");
        }
    }
}
