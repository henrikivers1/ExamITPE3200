using GalacticSlicer.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;
using GalacticSlicer.DAL;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);


// Creates a unique log file name using the current date and time.
// Example: logs/log-2026-09-30.txt
var logFileName = $"logs/log-{DateTime.Now:yyyy-MM-dd}.txt";


// Configures Serilog.
// WriteTo.File means that log messages will be saved in the file above.
// CreateLogger creates the logger with this configuration.
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.File(logFileName)
    .CreateLogger();


// Tells ASP.NET Core to use Serilog as the logging system.
builder.Host.UseSerilog();

builder.Services.AddDbContext<GalacticSlicerDbContext>(options =>
{
    options.UseSqlite(
        builder.Configuration["ConnectionStrings:GalacticSlicerDbContextConnection"] ?? throw new InvalidOperationException("Connection string 'GalacticSlicerDbContextConnection' not found."));
});
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false; // no email confirmation needed
})
.AddEntityFrameworkStores<GalacticSlicerDbContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";       // where [Authorize] sends users who aren't logged in
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/Login";
});
builder.Services.AddScoped<IChallengeRepository, ChallengeRepository>();
builder.Services.AddScoped<ICharacterRepository, CharacterRepository>();
builder.Services.AddScoped<IPlanetRepository, PlanetRepository>();

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    DBInit.Seed(app);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();   
app.UseAuthorization();    

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
