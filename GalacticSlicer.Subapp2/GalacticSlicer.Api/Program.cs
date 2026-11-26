using GalacticSlicer.Api.Models;
using GalacticSlicer.Api.DAL;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GalacticSlicer.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Register EF Core with the API's own SQLite database
builder.Services.AddDbContext<GalacticSlicerDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Register ASP.NET Core Identity with the same database
builder.Services
    .AddIdentityCore<IdentityUser>()
    .AddEntityFrameworkStores<GalacticSlicerDbContext>();

// Add services to the container.
// Register the planet repository for dependency injection
builder.Services.AddScoped<IPlanetRepository, PlanetRepository>();
builder.Services.AddScoped<PlayerProgressService>();
builder.Services.AddScoped<IPlanetService, PlanetService>();
builder.Services.AddControllers();
builder.Services.AddScoped<IChallengeRepository, ChallengeRepository>();
builder.Services.AddScoped<IChallengeService, ChallengeService>();
builder.Services.AddScoped<ICharacterRepository, CharacterRepository>();
builder.Services.AddScoped<ICharacterService, CharacterService>();
builder.Services.AddScoped<CharacterGameplayService>();
builder.Services
    .AddIdentityCore<IdentityUser>()
    .AddEntityFrameworkStores<GalacticSlicerDbContext>();

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT key is missing.");

var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException("JWT issuer is missing.");

var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException("JWT audience is missing.");

var keyBytes = Convert.FromBase64String(jwtKey);

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

                NameClaimType = "sub"
            };
    });

builder.Services.AddAuthorization();



// Allow requests from the React frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactClient", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
builder.Services.AddOpenApi();

var app = builder.Build();


// Seed initial planet data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider
        .GetRequiredService<GalacticSlicerDbContext>();

    await DBInit.SeedAsync(context);
}


// Configure the HTTP request pipeline.
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
