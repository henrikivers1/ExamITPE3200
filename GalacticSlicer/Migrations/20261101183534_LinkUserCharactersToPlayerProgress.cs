using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GalacticSlicer.Migrations
{
    /// <inheritdoc />
    public partial class LinkUserCharactersToPlayerProgress : Migration
    {
        /// <inheritdoc />

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add a nullable foreign key to preserve existing purchases
            migrationBuilder.AddColumn<int>(
                name: "PlayerProgressId",
                table: "UserCharacters",
                type: "INTEGER",
                nullable: true);

            // Connect existing purchases to their player's progress record
            migrationBuilder.Sql("""
        UPDATE UserCharacters
        SET PlayerProgressId = (
            SELECT p.Id
            FROM PlayerProgresses AS p
            WHERE p.UserId = UserCharacters.UserId
        )
        WHERE EXISTS (
            SELECT 1
            FROM PlayerProgresses AS p
            WHERE p.UserId = UserCharacters.UserId
        );
        """);

            // Create an index for the new relationship
            migrationBuilder.CreateIndex(
                name: "IX_UserCharacters_PlayerProgressId",
                table: "UserCharacters",
                column: "PlayerProgressId");

            // Connect UserCharacter to PlayerProgress
            migrationBuilder.AddForeignKey(
                name: "FK_UserCharacters_PlayerProgresses_PlayerProgressId",
                table: "UserCharacters",
                column: "PlayerProgressId",
                principalTable: "PlayerProgresses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserCharacters_PlayerProgresses_PlayerProgressId",
                table: "UserCharacters");

            migrationBuilder.DropIndex(
                name: "IX_UserCharacters_PlayerProgressId",
                table: "UserCharacters");

            migrationBuilder.DropColumn(
                name: "PlayerProgressId",
                table: "UserCharacters");
        }
    }
}
