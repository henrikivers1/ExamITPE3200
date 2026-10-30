using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GalacticSlicer.Migrations
{
    /// <inheritdoc />
    public partial class OptionalChallengePlanet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Challenges_Planets_PlanetId",
                table: "Challenges");

            migrationBuilder.AlterColumn<int>(
                name: "PlanetId",
                table: "Challenges",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddForeignKey(
                name: "FK_Challenges_Planets_PlanetId",
                table: "Challenges",
                column: "PlanetId",
                principalTable: "Planets",
                principalColumn: "PlanetId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Challenges_Planets_PlanetId",
                table: "Challenges");

            migrationBuilder.AlterColumn<int>(
                name: "PlanetId",
                table: "Challenges",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Challenges_Planets_PlanetId",
                table: "Challenges",
                column: "PlanetId",
                principalTable: "Planets",
                principalColumn: "PlanetId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
