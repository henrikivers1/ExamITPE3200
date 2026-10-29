using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GalacticSlicer.Migrations
{
    /// <inheritdoc />
    public partial class AddExperiencePoints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ExperiencePoints",
                table: "PlayerProgresses",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExperiencePoints",
                table: "PlayerProgresses");
        }
    }
}
