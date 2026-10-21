using GalacticSlicer.Models;

namespace GalacticSlicer.DAL;

public static class DBInit
{
    public static void Seed(IApplicationBuilder app)
    {
        using var serviceScope = app.ApplicationServices.CreateScope();
        var context = serviceScope.ServiceProvider.GetRequiredService<GalacticSlicerDbContext>();
        if (!context.Planets.Any())
        {
            var planets = new List<Planet>
    {
        new Planet { Name = "Tatooine",  Order = 1, CssClass = "tatooine",  Description = "A desert world where every slicer starts." },
        new Planet { Name = "Naboo",     Order = 2, CssClass = "naboo",     Description = "Peaceful on the surface, phishing underneath." },
        new Planet { Name = "Coruscant", Order = 3, CssClass = "coruscant", Description = "A city planet full of web applications." },
        new Planet { Name = "Kamino",    Order = 4, CssClass = "kamino",    Description = "Clone labs guarded by strict access control." },
        new Planet { Name = "Hoth",      Order = 5, CssClass = "hoth",      Description = "Frozen networks hide dangerous malware." },
        new Planet { Name = "Endor",     Order = 6, CssClass = "endor",     Description = "The final test for a legendary slicer." },
    };
            context.Planets.AddRange(planets);
            context.SaveChanges();
        }
        // Only add characters if the table is empty, so we never create duplicates
        if (!context.Characters.Any())
        {
            var characters = new List<Character>
            {
                new Character { Name = "Rookie Slicer", Price = 0, Description = "Every hacker starts somewhere. Free starter character." },
                new Character { Name = "Astromech Hacker", Price = 150, Description = "A droid that plugs straight into any terminal." },
                new Character { Name = "Smuggler", Price = 300, Description = "Knows every back door in the Outer Rim." },
                new Character { Name = "Bounty Hunter", Price = 500, Description = "Tracks down malware for a price." },
                new Character { Name = "Shadow Operative", Price = 1000, Description = "Leaves no logs behind. Legendary." },
            };
            context.Characters.AddRange(characters);
            context.SaveChanges();
        }
    }
}