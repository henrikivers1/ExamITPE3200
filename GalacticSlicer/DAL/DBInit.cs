using System.Text.Json;
using GalacticSlicer.Models;
using GalacticSlicer.Services;
using Microsoft.EntityFrameworkCore;

namespace GalacticSlicer.DAL;

public static class DBInit
{
    public static void Seed(IApplicationBuilder app)
    {
        using var serviceScope = app.ApplicationServices.CreateScope();
        var context = serviceScope.ServiceProvider.GetRequiredService<GalacticSlicerDbContext>();
        var env = serviceScope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

        // The .db file is not in Git, so create it and apply any new migrations on startup
        context.Database.Migrate();

        // Each planet's mission asks about one subject from questions.json.
        // Endor has no subject, so its mission mixes questions from every subject (the final test).
        var planets = new List<Planet>
        {
            new Planet { Name = "Tatooine",  Order = 1, CssClass = "tatooine",  Subject = "Linux Command Line",              Description = "A desert world where every slicer starts by mastering the command line." },
            new Planet { Name = "Naboo",     Order = 2, CssClass = "naboo",     Subject = "Network Security",                Description = "Peaceful on the surface, but its networks are full of open ports and eavesdroppers." },
            new Planet { Name = "Coruscant", Order = 3, CssClass = "coruscant", Subject = "Software Security",               Description = "A city planet full of web applications and the bugs hiding inside them." },
            new Planet { Name = "Kamino",    Order = 4, CssClass = "kamino",    Subject = "Access Control & Authentication", Description = "Clone labs guarded by strict access control." },
            new Planet { Name = "Hoth",      Order = 5, CssClass = "hoth",      Subject = "Cryptography Basics",             Description = "Frozen secrets locked away behind ciphers and keys." },
            new Planet { Name = "Endor",     Order = 6, CssClass = "endor",     Subject = null,                              Description = "The final test for a legendary slicer: questions from every subject." },
        };

        if (!context.Planets.Any())
        {
            context.Planets.AddRange(planets);
        }
        else
        {
            // Existing databases: keep the planets, but update their subject and description
            foreach (var seed in planets)
            {
                var planet = context.Planets.FirstOrDefault(p => p.Name == seed.Name);
                if (planet != null)
                {
                    planet.Subject = seed.Subject;
                    planet.Description = seed.Description;
                }
            }
        }
        context.SaveChanges();

        SeedQuestions(context, Path.Combine(env.ContentRootPath, "DAL", "SeedData", "questions.json"));

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

    // Imports questions.json into the Challenges table.
    // New questions are added and changed ones updated (matched by SourceId), so editing the
    // JSON file is enough: everyone gets the changes the next time they start the app.
    private static void SeedQuestions(GalacticSlicerDbContext context, string path)
    {
        if (!File.Exists(path))
        {
            return;
        }

        var seeds = JsonSerializer.Deserialize<List<QuestionSeed>>(
            File.ReadAllText(path),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<QuestionSeed>();

        // Cleanup: remove challenges that are neither from questions.json nor made by a user
        // (the old placeholder challenges), and the completions pointing to them.
        // Nobody can see or use them, since missions only use questions.json and users only see their own.
        var placeholders = context.Challenges
            .Where(c => c.SourceId == null && c.CreatedByUserId == null)
            .ToList();
        if (placeholders.Count > 0)
        {
            var placeholderIds = placeholders.Select(c => c.ChallengeId).ToList();
            context.CompletedChallenges.RemoveRange(
                context.CompletedChallenges.Where(cc => placeholderIds.Contains(cc.ChallengeId)));
            context.Challenges.RemoveRange(placeholders);
            context.SaveChanges();
        }

        var planets = context.Planets.ToList();
        // Subjects without their own planet are placed on the mixed planet (Endor)
        var mixedPlanet = planets.FirstOrDefault(p => p.Subject == null) ?? planets.OrderBy(p => p.Order).Last();
        var existing = context.Challenges.Where(c => c.SourceId != null).ToDictionary(c => c.SourceId!);

        foreach (var seed in seeds)
        {
            // Skip anything that doesn't fit a 4-option question with one correct answer
            if (seed.Options.Count != 4 || seed.CorrectIndex < 0 || seed.CorrectIndex > 3)
            {
                continue;
            }

            if (!existing.TryGetValue(seed.Id, out var challenge))
            {
                challenge = new Challenge { SourceId = seed.Id, CreditReward = 100 };
                context.Challenges.Add(challenge);
            }

            challenge.Subject = seed.Subject;
            challenge.Title = QuestionService.MakeTitle(seed.Question);
            challenge.Question = seed.Question;
            challenge.OptionA = seed.Options[0];
            challenge.OptionB = seed.Options[1];
            challenge.OptionC = seed.Options[2];
            challenge.OptionD = seed.Options[3];
            challenge.CorrectAnswer = seed.Options[seed.CorrectIndex];
            challenge.Explanation = seed.Explanation;
            challenge.PlanetId = (planets.FirstOrDefault(p => p.Subject == seed.Subject) ?? mixedPlanet).PlanetId;
        }
        context.SaveChanges();
    }

    // One question as written in questions.json
    private class QuestionSeed
    {
        public string Id { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Question { get; set; } = string.Empty;
        public List<string> Options { get; set; } = new();
        public int CorrectIndex { get; set; }
        public string? Explanation { get; set; }
    }
}
