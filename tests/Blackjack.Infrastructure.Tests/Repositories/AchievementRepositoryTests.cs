namespace Blackjack.Infrastructure.Tests.Repositories;

using Blackjack.Domain.Models;
using Blackjack.Infrastructure.Data;
using Blackjack.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class AchievementRepositoryTests
{
    private static BlackjackDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<BlackjackDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new BlackjackDbContext(options);
    }

    private static async Task<BlackjackDbContext> CreateContextWithUser(string dbName, string userId = "user1")
    {
        var context = CreateContext(dbName);
        var user = new ApplicationUser
        {
            Id = userId,
            UserName = "test@test.com",
            Email = "test@test.com",
            Balance = 1000m
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return context;
    }

    private static GameRecord CreateGameRecord(string userId, GameResult result, decimal bet = 100m, decimal payout = 200m) => new()
    {
        UserId = userId,
        StartedAt = DateTime.UtcNow.AddMinutes(-5),
        EndedAt = DateTime.UtcNow,
        InitialBet = bet,
        FinalPayout = payout,
        Result = result,
        PlayerHandSummary = "A♠ K♥",
        DealerHandSummary = "10♣ 7♦",
        PlayerScore = 21,
        DealerScore = 17
    };

    [Fact]
    public async Task GetEarnedAchievementIdsAsync_ReturnsEmptyForNewUser()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var context = await CreateContextWithUser(dbName);
        var repo = new AchievementRepository(context);

        var ids = await repo.GetEarnedAchievementIdsAsync("user1");

        Assert.Empty(ids);
    }

    [Fact]
    public async Task GetEarnedAchievementsWithDatesAsync_ReturnsEmptyForNewUser()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var context = await CreateContextWithUser(dbName);
        var repo = new AchievementRepository(context);

        var dict = await repo.GetEarnedAchievementsWithDatesAsync("user1");

        Assert.Empty(dict);
    }

    [Fact]
    public async Task CheckAndAwardNewAchievementsAsync_ReturnsEmptyForUserWithNoGames()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var context = await CreateContextWithUser(dbName);
        var repo = new AchievementRepository(context);

        var newIds = await repo.CheckAndAwardNewAchievementsAsync("user1");

        Assert.Empty(newIds);
    }

    [Fact]
    public async Task CheckAndAwardNewAchievementsAsync_AwardsFirstWinAfterWin()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var context = await CreateContextWithUser(dbName);
        context.GameRecords.Add(CreateGameRecord("user1", GameResult.Win));
        await context.SaveChangesAsync();
        var repo = new AchievementRepository(context);

        var newIds = await repo.CheckAndAwardNewAchievementsAsync("user1");

        Assert.Contains("first_win", newIds);
        Assert.Single(await context.UserAchievements.Where(a => a.UserId == "user1").ToListAsync());
    }

    [Fact]
    public async Task CheckAndAwardNewAchievementsAsync_AwardsNaturalAfterBlackjack()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var context = await CreateContextWithUser(dbName);
        context.GameRecords.Add(CreateGameRecord("user1", GameResult.Blackjack, 100m, 250m));
        await context.SaveChangesAsync();
        var repo = new AchievementRepository(context);

        var newIds = await repo.CheckAndAwardNewAchievementsAsync("user1");

        Assert.Contains("natural", newIds);
        Assert.Contains("first_win", newIds);
    }

    [Fact]
    public async Task CheckAndAwardNewAchievementsAsync_DoesNotDuplicateAlreadyEarnedAchievements()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var context = await CreateContextWithUser(dbName);
        context.GameRecords.Add(CreateGameRecord("user1", GameResult.Win));
        await context.SaveChangesAsync();
        var repo = new AchievementRepository(context);

        // First check awards the achievement
        var firstCall = await repo.CheckAndAwardNewAchievementsAsync("user1");
        Assert.Contains("first_win", firstCall);

        // Second check should not re-award
        context.GameRecords.Add(CreateGameRecord("user1", GameResult.Win));
        await context.SaveChangesAsync();
        var secondCall = await repo.CheckAndAwardNewAchievementsAsync("user1");
        Assert.DoesNotContain("first_win", secondCall);
    }

    [Fact]
    public async Task CheckAndAwardNewAchievementsAsync_AwardsHotStreakForFiveConsecutiveWins()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var context = await CreateContextWithUser(dbName);
        var baseTime = DateTime.UtcNow.AddMinutes(-60);
        for (int i = 0; i < 5; i++)
        {
            context.GameRecords.Add(new GameRecord
            {
                UserId = "user1",
                StartedAt = baseTime.AddMinutes(i * 5),
                EndedAt = baseTime.AddMinutes(i * 5 + 4),
                InitialBet = 100m,
                FinalPayout = 200m,
                Result = GameResult.Win,
                PlayerHandSummary = "A♠ K♥",
                DealerHandSummary = "10♣ 7♦",
                PlayerScore = 21,
                DealerScore = 17
            });
        }
        await context.SaveChangesAsync();
        var repo = new AchievementRepository(context);

        var newIds = await repo.CheckAndAwardNewAchievementsAsync("user1");

        Assert.Contains("hot_streak", newIds);
    }

    [Fact]
    public async Task GetEarnedAchievementsWithDatesAsync_ReturnsDictionaryWithEarnedDates()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var context = await CreateContextWithUser(dbName);
        var earnedAt = new DateTime(2024, 6, 15, 12, 0, 0, DateTimeKind.Utc);
        context.UserAchievements.Add(new UserAchievement
        {
            UserId = "user1",
            AchievementId = "first_win",
            EarnedAt = earnedAt
        });
        await context.SaveChangesAsync();
        var repo = new AchievementRepository(context);

        var dict = await repo.GetEarnedAchievementsWithDatesAsync("user1");

        Assert.Single(dict);
        Assert.True(dict.ContainsKey("first_win"));
        Assert.Equal(earnedAt, dict["first_win"]);
    }

    [Fact]
    public async Task CheckAndAwardNewAchievementsAsync_AwardsHighRollerWhenWageredOver5000()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var context = await CreateContextWithUser(dbName);
        // 50 games at $100 each = $5000 wagered
        var baseTime = DateTime.UtcNow.AddDays(-10);
        for (int i = 0; i < 50; i++)
        {
            context.GameRecords.Add(new GameRecord
            {
                UserId = "user1",
                StartedAt = baseTime.AddMinutes(i * 5),
                EndedAt = baseTime.AddMinutes(i * 5 + 4),
                InitialBet = 100m,
                FinalPayout = 0m,
                Result = GameResult.Lose,
                PlayerHandSummary = "5♠ 10♥",
                DealerHandSummary = "10♣ 7♦",
                PlayerScore = 15,
                DealerScore = 17
            });
        }
        await context.SaveChangesAsync();
        var repo = new AchievementRepository(context);

        var newIds = await repo.CheckAndAwardNewAchievementsAsync("user1");

        Assert.Contains("high_roller", newIds);
    }
}
