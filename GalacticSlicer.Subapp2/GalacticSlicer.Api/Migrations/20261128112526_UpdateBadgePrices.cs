using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GalacticSlicer.Api.Migrations
{
    // Data migration: the standard badges now cost 500 / 1000 / 1500 / 2000 like Subapp 1
    // (one tier per badge, and each passed mission pays for the next one).
    // DBInit only adds missing characters, so databases created before the change
    // still had 150 / 300 / 500 / 1000. Only rows that still have the old price are
    // changed, so a price an admin set on purpose is left alone.
    // On a new database this runs before the seeding and changes nothing.
    /// <inheritdoc />
    public partial class UpdateBadgePrices : Migration
    {
        private static readonly (string Name, int OldPrice, int NewPrice)[] Prices =
        {
            ("Astromech Hacker", 150, 500),
            ("Smuggler", 300, 1000),
            ("Bounty Hunter", 500, 1500),
            ("Shadow Operative", 1000, 2000)
        };

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            foreach (var (name, oldPrice, newPrice) in Prices)
            {
                migrationBuilder.Sql(
                    $"UPDATE \"Characters\" SET \"Price\" = {newPrice} " +
                    $"WHERE \"Name\" = '{name}' AND \"Price\" = {oldPrice};");
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            foreach (var (name, oldPrice, newPrice) in Prices)
            {
                migrationBuilder.Sql(
                    $"UPDATE \"Characters\" SET \"Price\" = {oldPrice} " +
                    $"WHERE \"Name\" = '{name}' AND \"Price\" = {newPrice};");
            }
        }
    }
}
