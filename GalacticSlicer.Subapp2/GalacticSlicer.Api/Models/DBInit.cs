
using Microsoft.EntityFrameworkCore;

namespace GalacticSlicer.Api.Models;

public static class DBInit
{
    public static async Task SeedAsync(
        GalacticSlicerDbContext context)
    {
        // Avoid inserting duplicate seed data
        if (await context.Planets.AnyAsync())
        {
            return;
        }

        var planets = new List<Planet>
        {
            new Planet
            {
                Name = "Tatooine",
                Description = "Your first cybersecurity mission.",
                Order = 1,
                CssClass = "tatooine",
                Subject = null
            },
            new Planet
            {
                Name = "Coruscant",
                Description = "Continue your cybersecurity training.",
                Order = 2,
                CssClass = "coruscant",
                Subject = null
            }
        };

        context.Planets.AddRange(planets);

        await context.SaveChangesAsync();
    }
}
