
using System.Security.Claims;
using System.Security.Cryptography;
using GalacticSlicer.Api.Models;
using GalacticSlicer.Api.DAL;
using GalacticSlicer.Api.Filters;
using GalacticSlicer.Api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ============================================
// DATABASE - Entity Framework Core + SQLite
// ============================================

builder.Services.AddDbContext<GalacticSlicerDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// ============================================
// ASP.NET Core Identity
// ============================================

builder.Services
    .AddIdentityCore<IdentityUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<GalacticSlicerDbContext>();

// ============================================
// Controllers
// ============================================

// LogExceptionFilter logs every unhandled controller error and returns a 500 with a message
builder.Services.AddControllers(options =>
    options.Filters.Add<LogExceptionFilter>());
builder.Services.AddOpenApi();

// ============================================
// Repositories
// ============================================

builder.Services.AddScoped<IPlanetRepository, PlanetRepository>();
builder.Services.AddScoped<IChallengeRepository, ChallengeRepository>();
builder.Services.AddScoped<ICharacterRepository, CharacterRepository>();

// ============================================
// CRUD Services
// ============================================

builder.Services.AddScoped<IPlanetService, PlanetService>();
builder.Services.AddScoped<IChallengeService, ChallengeService>();
builder.Services.AddScoped<ICharacterService, CharacterService>();

// ============================================
// Gameplay Services
// ============================================

builder.Services.AddScoped<PlayerProgressService>();
builder.Services.AddScoped<CharacterGameplayService>();
builder.Services.AddScoped<ChallengeCompletionService>();
builder.Services.AddScoped<PlanetProgressService>();
builder.Services.AddScoped<RankService>();

// Remembers the questions each mission handed out (see MissionQuestionStore)
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<MissionQuestionStore>();

// ============================================
// JWT Authentication
// ============================================

var jwtKey = builder.Configuration["Jwt:Key"];

if (string.IsNullOrWhiteSpace(jwtKey))
{
    if (!builder.Environment.IsDevelopment())
    {
        throw new InvalidOperationException(
            "JWT key is missing. Configure Jwt:Key using User Secrets or environment variables.");
    }

    // Generate a temporary development key if none is configured.
    // Tokens become invalid when the API restarts.
    jwtKey = Convert.ToBase64String(
        RandomNumberGenerator.GetBytes(32));

    // Make the generated key available to AuthController.
    builder.Configuration["Jwt:Key"] = jwtKey;
}

var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException(
        "JWT issuer is missing.");

var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException(
        "JWT audience is missing.");

byte[] keyBytes;

try
{
    keyBytes = Convert.FromBase64String(jwtKey);
}
catch (FormatException ex)
{
    throw new InvalidOperationException(
        "Jwt:Key must be a Base64-encoded value.", ex);
}

if (keyBytes.Length < 32)
{
    throw new InvalidOperationException(
        "JWT key must be at least 32 bytes.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;

        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,

                ValidateAudience = true,
                ValidAudience = jwtAudience,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey =
                    new SymmetricSecurityKey(keyBytes),

                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,

                NameClaimType = "sub",
                RoleClaimType = ClaimTypes.Role
            };
    });

builder.Services.AddAuthorization();

// ============================================
// CORS - React Frontend
// ============================================

builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactClient", policy =>
    {
        var origins = builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>()
            ?? new[] { "http://localhost:5173" };

        policy.WithOrigins(origins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ============================================
// Build Application
// ============================================

var app = builder.Build();

// ============================================
// Database Migration + Seed Data
// ============================================

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider
        .GetRequiredService<GalacticSlicerDbContext>();

    // Apply existing migrations without deleting the database.
    await context.Database.MigrateAsync();

    // Seed six planets, five starter characters,
    // and the official questions from Questions.json.
    await DBInit.SeedAsync(context);

    // Create the Admin role and optionally bootstrap
    // an administrator account using configuration.
    var logger = scope.ServiceProvider
        .GetRequiredService<ILogger<Program>>();

    await DBInit.SeedAdminAsync(
        scope.ServiceProvider,
        app.Configuration,
        logger);
}

// ============================================
// HTTP Pipeline
// ============================================

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("ReactClient");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
