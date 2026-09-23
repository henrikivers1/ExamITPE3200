using ExamITPE3200.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);


// Creates a unique log file name using the current date and time.
// Example: logs/log-20260923_103500.txt
var logFileName = $"logs/log-{DateTime.Now:yyyy-MM-dd}.txt";


// Configures Serilog.
// WriteTo.File means that log messages will be saved in the file above.
// CreateLogger creates the logger with this configuration.
Log.Logger = new LoggerConfiguration()
    .WriteTo.File(logFileName)
    .CreateLogger();


// Tells ASP.NET Core to use Serilog as the logging system.
builder.Host.UseSerilog();

builder.Services.AddDbContext<GalacticSlicerDbContext>(options =>
{
    options.UseSqlite(
        builder.Configuration["ConnectionStrings:GalacticSlicerDbContextConnection"] ?? throw new InvalidOperationException("Connection string 'GalacticSlicerDbContextConnection' not found."));
});


// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
