using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace GalacticSlicer.Models;
public class GalacticSlicerDbContext : IdentityDbContext
{
    public GalacticSlicerDbContext(DbContextOptions<GalacticSlicerDbContext> options) : base(options)
    {
    }

    public DbSet<Challenge> Challenges { get; set; }
    public DbSet<Character> Characters { get; set; }
    
}