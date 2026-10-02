using Microsoft.EntityFrameworkCore;

namespace GalacticSlicer.Models;
public class GalacticSlicerDbContext : DbContext
{
    public GalacticSlicerDbContext(DbContextOptions<GalacticSlicerDbContext> options) : base(options)
    {
    }

    public DbSet<Challenge> Challenges { get; set; }
}