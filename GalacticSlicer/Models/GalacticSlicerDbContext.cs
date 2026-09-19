using Microsoft.EntityFrameworkCore;

namespace ExamITPE3200.Models;
public class GalacticSlicerDbContext : DbContext
{
    public GalacticSlicerDbContext(DbContextOptions<GalacticSlicerDbContext> options) : base(options)
    {
    }

    public DbSet<Challenge> Challenges { get; set; }
}