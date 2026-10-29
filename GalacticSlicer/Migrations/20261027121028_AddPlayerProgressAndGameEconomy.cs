using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GalacticSlicer.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayerProgressAndGameEconomy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CompletedChallenges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    ChallengeId = table.Column<int>(type: "INTEGER", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompletedChallenges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompletedChallenges_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompletedChallenges_Challenges_ChallengeId",
                        column: x => x.ChallengeId,
                        principalTable: "Challenges",
                        principalColumn: "ChallengeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlayerProgresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    Credits = table.Column<int>(type: "INTEGER", nullable: false),
                    Rank = table.Column<string>(type: "TEXT", nullable: false),
                    ActiveCharacterId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerProgresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerProgresses_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerProgresses_Characters_ActiveCharacterId",
                        column: x => x.ActiveCharacterId,
                        principalTable: "Characters",
                        principalColumn: "CharacterId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "UserCharacters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    CharacterId = table.Column<int>(type: "INTEGER", nullable: false),
                    PurchasedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCharacters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserCharacters_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserCharacters_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "CharacterId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompletedChallenges_ChallengeId",
                table: "CompletedChallenges",
                column: "ChallengeId");

            migrationBuilder.CreateIndex(
                name: "IX_CompletedChallenges_UserId_ChallengeId",
                table: "CompletedChallenges",
                columns: new[] { "UserId", "ChallengeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlayerProgresses_ActiveCharacterId",
                table: "PlayerProgresses",
                column: "ActiveCharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerProgresses_UserId",
                table: "PlayerProgresses",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserCharacters_CharacterId",
                table: "UserCharacters",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCharacters_UserId_CharacterId",
                table: "UserCharacters",
                columns: new[] { "UserId", "CharacterId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompletedChallenges");

            migrationBuilder.DropTable(
                name: "PlayerProgresses");

            migrationBuilder.DropTable(
                name: "UserCharacters");
        }
    }
}
