using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GalacticSlicer.Migrations
{
    /// <inheritdoc />
    public partial class ConnectChallengeToPlanet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlanetName",
                table: "Challenges");

            migrationBuilder.AddColumn<int>(
                name: "PlanetId",
                table: "Challenges",
                type: "INTEGER",
                maxLength: 100,
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Challenges_PlanetId",
                table: "Challenges",
                column: "PlanetId");

            migrationBuilder.AddForeignKey(
                name: "FK_Challenges_Planets_PlanetId",
                table: "Challenges",
                column: "PlanetId",
                principalTable: "Planets",
                principalColumn: "PlanetId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Challenges_Planets_PlanetId",
                table: "Challenges");

            migrationBuilder.DropIndex(
                name: "IX_Challenges_PlanetId",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "PlanetId",
                table: "Challenges");

            migrationBuilder.AddColumn<string>(
                name: "PlanetName",
                table: "Challenges",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
