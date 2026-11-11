using GalacticSlicer.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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

builder.Services.AddControllers();

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
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
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

app.UseAuthorization();

app.MapControllers();
app.Run();
