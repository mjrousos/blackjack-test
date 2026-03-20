namespace Blackjack.Infrastructure.Data;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class BlackjackDbContext : IdentityDbContext<ApplicationUser>
{
    public DbSet<GameRecord> GameRecords => Set<GameRecord>();
    public DbSet<UserAchievement> UserAchievements => Set<UserAchievement>();

    public BlackjackDbContext(DbContextOptions<BlackjackDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(u => u.Balance).HasPrecision(18, 2);
        });

        builder.Entity<GameRecord>(entity =>
        {
            entity.HasIndex(g => new { g.UserId, g.StartedAt });
            entity.HasOne(g => g.User)
                  .WithMany()
                  .HasForeignKey(g => g.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.Property(g => g.InitialBet).HasPrecision(18, 2);
            entity.Property(g => g.FinalPayout).HasPrecision(18, 2);
            entity.Property(g => g.Result).HasConversion<string>();
        });

        builder.Entity<UserAchievement>(entity =>
        {
            entity.HasIndex(a => new { a.UserId, a.AchievementId }).IsUnique();
            entity.HasOne(a => a.User)
                  .WithMany()
                  .HasForeignKey(a => a.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
