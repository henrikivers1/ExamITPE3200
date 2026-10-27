using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace GalacticSlicer.Models;

public class GalacticSlicerDbContext : IdentityDbContext
{
    public GalacticSlicerDbContext
       (DbContextOptions<GalacticSlicerDbContext> options)
        : base(options)
    {
    }

    public DbSet<Challenge> Challenges { get; set; }
    public DbSet<Character> Characters { get; set; }
    public DbSet<Planet> Planets { get; set; }

    public DbSet<PlayerProgress> PlayerProgresses { get; set; }
    public DbSet<UserCharacter> UserCharacters { get; set; }
    public DbSet<CompletedChallenge> CompletedChallenges { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);


     // Each user can have only one progress record
        modelBuilder.Entity<PlayerProgress>()
            .HasIndex(p => p.UserId)
            .IsUnique();

    // Prevent users from owning the same character twice
        modelBuilder.Entity<UserCharacter>()
            .HasIndex(uc => new { uc.UserId, uc.CharacterId })
            .IsUnique();

    // Prevent users from completing the same challenge twice
        modelBuilder.Entity<CompletedChallenge>()
            .HasIndex(cc => new { cc.UserId, cc.ChallengeId })
            .IsUnique();

    // A character can be equipped by multiple players
        modelBuilder.Entity<PlayerProgress>()
            .HasOne(p => p.ActiveCharacter)
            .WithMany()
            .HasForeignKey(p => p.ActiveCharacterId)
            .OnDelete(DeleteBehavior.SetNull);

    // Connect player progress to ASP.NET Identity
        modelBuilder.Entity<PlayerProgress>()
            .HasOne<IdentityUser>()
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);   
    
      // Owned character relationship
        modelBuilder.Entity<UserCharacter>()
            .HasOne(uc => uc.Character)
            .WithMany()
            .HasForeignKey(uc => uc.CharacterId)
            .OnDelete(DeleteBehavior.Restrict);
        
    // Connect owned characters to ASP.NET Identity
        modelBuilder.Entity<UserCharacter>()
            .HasOne<IdentityUser>()
            .WithMany()
            .HasForeignKey(uc => uc.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
    // Completed challenge relationship
        modelBuilder.Entity<CompletedChallenge>()
            .HasOne(cc => cc.Challenge)
            .WithMany()
            .HasForeignKey(cc => cc.ChallengeId)
            .OnDelete(DeleteBehavior.Restrict);
    // Connect completed challenges to ASP.NET Identity
        modelBuilder.Entity<CompletedChallenge>()
            .HasOne<IdentityUser>()
            .WithMany()
            .HasForeignKey(cc => cc.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    
    }

}