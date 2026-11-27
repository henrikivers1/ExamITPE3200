
using System.Security.Claims;
using System.Text.Json;
using GalacticSlicer.Api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GalacticSlicer.Api.Models;

public static class DBInit
{
    public static async Task SeedAsync(
        GalacticSlicerDbContext context)
    {
        await SeedPlanetsAsync(context);
        await SeedCharactersAsync(context);
        await SeedQuestionsAsync(context);
    }

    // ============================================
    // ADMIN ROLE + ADMIN USER
    // ============================================

    public static async Task SeedAdminAsync(
        IServiceProvider services,
        IConfiguration configuration,
        ILogger logger)
    {
        var roleManager = services
            .GetRequiredService<RoleManager<IdentityRole>>();

        var userManager = services
            .GetRequiredService<UserManager<IdentityUser>>();

        const string adminRole = "Admin";

        // Create the Admin role if it does not exist.
        if (!await roleManager.RoleExistsAsync(adminRole))
        {
            var roleResult = await roleManager.CreateAsync(
                new IdentityRole(adminRole));

            if (!roleResult.Succeeded)
            {
                var errors = string.Join(", ",
                    roleResult.Errors.Select(e => e.Description));

                throw new InvalidOperationException(
                    $"Failed to create Admin role: {errors}");
            }

            logger.LogInformation(
                "Admin role created successfully.");
        }

        // Read administrator credentials from configuration.
        var adminEmail = configuration["BootstrapAdmin:Email"];
        var adminPassword = configuration["BootstrapAdmin:Password"];

        // Skip account creation if no credentials are configured.
        if (string.IsNullOrWhiteSpace(adminEmail) &&
            string.IsNullOrWhiteSpace(adminPassword))
        {
            logger.LogInformation(
                "Admin bootstrap credentials not configured. " +
                "Skipping administrator account creation.");

            return;
        }

        if (string.IsNullOrWhiteSpace(adminEmail) ||
            string.IsNullOrWhiteSpace(adminPassword))
        {
            throw new InvalidOperationException(
                "Both BootstrapAdmin:Email and " +
                "BootstrapAdmin:Password must be configured.");
        }

        var admin = await userManager.FindByEmailAsync(adminEmail);

        if (admin == null)
        {
            admin = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail
            };

            var createResult = await userManager.CreateAsync(
                admin,
                adminPassword);

            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ",
                    createResult.Errors.Select(e => e.Description));

                throw new InvalidOperationException(
                    $"Failed to create administrator: {errors}");
            }

            logger.LogInformation(
                "Administrator account created.");
        }
        else
        {
            // Never promote an existing ordinary account automatically.
            if (!await userManager.IsInRoleAsync(admin, adminRole))
            {
                throw new InvalidOperationException(
                    "Bootstrap admin email already belongs to " +
                    "a non-admin account. Configure a different " +
                    "email to create a new administrator.");
            }

            // Never reset an existing administrator password.
            logger.LogInformation(
                "Administrator account already exists.");
        }

        if (!await userManager.IsInRoleAsync(admin, adminRole))
        {
            var roleResult = await userManager.AddToRoleAsync(
                admin,
                adminRole);

            if (!roleResult.Succeeded)
            {
                var errors = string.Join(", ",
                    roleResult.Errors.Select(e => e.Description));

                throw new InvalidOperationException(
                    $"Failed to assign Admin role: {errors}");
            }

            logger.LogInformation(
                "Admin role assigned successfully.");
        }

        // Give the administrator test Credits only once.
        await GrantAdminTestCreditsOnceAsync(
            services,
            userManager,
            admin,
            logger);
    }

    // ============================================
    // ADMIN TEST CREDITS
    // ============================================

    private static async Task GrantAdminTestCreditsOnceAsync(
        IServiceProvider services,
        UserManager<IdentityUser> userManager,
        IdentityUser admin,
        ILogger logger)
    {
        const string markerType =
            "galactic_slicer_admin_test_credits_v1";

        const int startingCredits = 1_000_000;

        // Check whether Admin has already received the bonus.
        var claims = await userManager.GetClaimsAsync(admin);

        if (claims.Any(c => c.Type == markerType))
        {
            return;
        }

        var progressService = services
            .GetRequiredService<PlayerProgressService>();

        // Ensure that the Admin has a PlayerProgress record.
        await progressService.GetOrCreateProgressAsync(admin.Id);

        var context = services
            .GetRequiredService<GalacticSlicerDbContext>();

        var progress = await context.PlayerProgresses
            .SingleAsync(p => p.UserId == admin.Id);

        // Preserve a higher existing balance.
        progress.Credits = Math.Max(
            progress.Credits,
            startingCredits);

        await context.SaveChangesAsync();

        // Record that the one-time bonus has been granted.
        var claimResult = await userManager.AddClaimAsync(
            admin,
            new Claim(markerType, "granted"));

        if (!claimResult.Succeeded)
        {
            var errors = string.Join(
                ", ",
                claimResult.Errors.Select(e => e.Description));

            throw new InvalidOperationException(
                $"Failed to record Admin credits grant: {errors}");
        }

        logger.LogInformation(
            "One-time Admin test credits granted.");
    }

    // ============================================
    // PLANETS
    // ============================================

    private static async Task SeedPlanetsAsync(
        GalacticSlicerDbContext context)
    {
        var seedPlanets = new List<Planet>
        {
            new()
            {
                Name = "Tatooine",
                Order = 1,
                CssClass = "tatooine",
                Subject = "Linux Command Line",
                Description = "A desert world where every slicer starts by mastering the command line."
            },
            new()
            {
                Name = "Naboo",
                Order = 2,
                CssClass = "naboo",
                Subject = "Network Security",
                Description = "Peaceful on the surface, but its networks are full of open ports and eavesdroppers."
            },
            new()
            {
                Name = "Coruscant",
                Order = 3,
                CssClass = "coruscant",
                Subject = "Software Security",
                Description = "A city planet full of web applications and the bugs hiding inside them."
            },
            new()
            {
                Name = "Kamino",
                Order = 4,
                CssClass = "kamino",
                Subject = "Access Control & Authentication",
                Description = "Clone labs guarded by strict access control."
            },
            new()
            {
                Name = "Hoth",
                Order = 5,
                CssClass = "hoth",
                Subject = "Cryptography Basics",
                Description = "Frozen secrets locked away behind ciphers and keys."
            },
            new()
            {
                Name = "Endor",
                Order = 6,
                CssClass = "endor",
                Subject = null,
                Description = "The final test for a legendary slicer: questions from every subject."
            }
        };

        var existingPlanets = await context.Planets.ToListAsync();

        foreach (var seed in seedPlanets)
        {
            var existing = existingPlanets.FirstOrDefault(
                p => string.Equals(
                    p.Name,
                    seed.Name,
                    StringComparison.OrdinalIgnoreCase));

            if (existing == null)
            {
                context.Planets.Add(seed);
            }
            else
            {
                // Repair old seed data without changing PlanetId.
                existing.Order = seed.Order;
                existing.CssClass = seed.CssClass;
                existing.Subject = seed.Subject;
                existing.Description = seed.Description;
            }
        }

        await context.SaveChangesAsync();
    }

    // ============================================
    // CHARACTERS
    // ============================================

    private static async Task SeedCharactersAsync(
        GalacticSlicerDbContext context)
    {
        var seedCharacters = new List<Character>
        {
            new()
            {
                Name = "Rookie Slicer",
                Price = 0,
                Description = "Every hacker starts somewhere. Free starter character."
            },
            new()
            {
                Name = "Astromech Hacker",
                Price = 150,
                Description = "A droid that plugs straight into any terminal."
            },
            new()
            {
                Name = "Smuggler",
                Price = 300,
                Description = "Knows every back door in the Outer Rim."
            },
            new()
            {
                Name = "Bounty Hunter",
                Price = 500,
                Description = "Tracks down malware for a price."
            },
            new()
            {
                Name = "Shadow Operative",
                Price = 1000,
                Description = "Leaves no logs behind. Legendary."
            }
        };

        var existingNames = await context.Characters
            .Select(c => c.Name)
            .ToListAsync();

        foreach (var character in seedCharacters)
        {
            if (!existingNames.Any(name =>
                string.Equals(
                    name,
                    character.Name,
                    StringComparison.OrdinalIgnoreCase)))
            {
                context.Characters.Add(character);
            }
        }

        await context.SaveChangesAsync();
    }

    // ============================================
    // OFFICIAL QUESTIONS
    // ============================================

    private static async Task SeedQuestionsAsync(
        GalacticSlicerDbContext context)
    {
        // Questions.json is copied to the API output
        // directory by GalacticSlicer.Api.csproj.
        var path = Path.Combine(
            AppContext.BaseDirectory,
            "SeedData",
            "Questions.json");

        if (!File.Exists(path))
        {
            throw new FileNotFoundException(
                "Official questions seed file was not found.",
                path);
        }

        var json = await File.ReadAllTextAsync(path);

        var seeds = JsonSerializer.Deserialize<List<QuestionSeed>>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<QuestionSeed>();

        var planets = await context.Planets.ToListAsync();

        var endor = planets.FirstOrDefault(
            p => p.Name == "Endor");

        if (endor == null)
        {
            throw new InvalidOperationException(
                "Endor must exist before questions are seeded.");
        }

        var existingQuestions = await context.Challenges
            .Where(c => c.SourceId != null)
            .ToDictionaryAsync(c => c.SourceId!);

        var processedIds = new HashSet<string>();

        foreach (var seed in seeds)
        {
            if (string.IsNullOrWhiteSpace(seed.Id) ||
                string.IsNullOrWhiteSpace(seed.Question) ||
                string.IsNullOrWhiteSpace(seed.Subject) ||
                seed.Options == null ||
                seed.Options.Count != 4 ||
                seed.CorrectIndex < 0 ||
                seed.CorrectIndex >= 4)
            {
                throw new InvalidOperationException(
                    $"Invalid question seed: {seed.Id}");
            }

            if (!processedIds.Add(seed.Id))
            {
                throw new InvalidOperationException(
                    $"Duplicate question SourceId: {seed.Id}");
            }

            var planet = planets.FirstOrDefault(
                p => p.Subject == seed.Subject) ?? endor;

            if (!existingQuestions.TryGetValue(
                seed.Id,
                out var challenge))
            {
                challenge = new Challenge
                {
                    SourceId = seed.Id,
                    CreditReward = 100
                };

                context.Challenges.Add(challenge);
            }

            // Create a short title from the question.
            var title = seed.Question.Trim();

            if (title.Length > 100)
            {
                title = title[..97] + "...";
            }

            challenge.Title = title;
            challenge.Question = seed.Question;
            challenge.Subject = seed.Subject;

            challenge.OptionA = seed.Options[0];
            challenge.OptionB = seed.Options[1];
            challenge.OptionC = seed.Options[2];
            challenge.OptionD = seed.Options[3];

            challenge.CorrectAnswer =
                seed.Options[seed.CorrectIndex];

            challenge.Explanation = seed.Explanation;

            challenge.PlanetId = planet.PlanetId;

            // Official questions are not owned by a user.
            challenge.CreatedByUserId = null;
        }

        await context.SaveChangesAsync();
    }

    // ============================================
    // JSON MODEL
    // ============================================

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
