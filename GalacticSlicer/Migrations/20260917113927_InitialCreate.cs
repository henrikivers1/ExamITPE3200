using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GalacticSlicer.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Challenges",
                columns: table => new
                {
                    ChallengeId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Question = table.Column<string>(type: "TEXT", nullable: false),
                    CorrectAnswer = table.Column<string>(type: "TEXT", nullable: false),
                    CreditReward = table.Column<int>(type: "INTEGER", nullable: false),
                    PlanetName = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Challenges", x => x.ChallengeId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Challenges");
        }
    }
}
