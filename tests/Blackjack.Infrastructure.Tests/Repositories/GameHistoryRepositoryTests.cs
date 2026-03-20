namespace Blackjack.Infrastructure.Tests.Repositories;

using Blackjack.Domain.Models;
using Blackjack.Infrastructure.Data;
using Blackjack.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class GameHistoryRepositoryTests
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

    private static GameRecord CreateGameRecord(string userId, GameResult result, decimal bet, decimal payout, DateTime startedAt) => new()
    {
        UserId = userId,
        StartedAt = startedAt,
        EndedAt = startedAt.AddMinutes(5),
        InitialBet = bet,
        FinalPayout = payout,
        Result = result,
        PlayerHandSummary = "A♠ K♥",
        DealerHandSummary = "10♣ 7♦",
        PlayerScore = 21,
        DealerScore = 17
    };

    [Fact]
    public async Task SaveGameRecordAsync_PersistsRecord()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var context = await CreateContextWithUser(dbName);
        var repo = new GameHistoryRepository(context);
        var record = CreateGameRecord("user1", GameResult.Win, 100m, 200m, DateTime.UtcNow);

        await repo.SaveGameRecordAsync(record);

        var saved = await context.GameRecords.FirstOrDefaultAsync(g => g.UserId == "user1");
        Assert.NotNull(saved);
        Assert.Equal(100m, saved.InitialBet);
        Assert.Equal(200m, saved.FinalPayout);
        Assert.Equal(GameResult.Win, saved.Result);
    }

    [Fact]
    public async Task GetHistoryAsync_ReturnsPaginatedResultsOrderedByDateDesc()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var context = await CreateContextWithUser(dbName);
        var baseDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        for (int i = 0; i < 5; i++)
        {
            context.GameRecords.Add(CreateGameRecord("user1", GameResult.Win, 100m, 200m, baseDate.AddDays(i)));
        }
        await context.SaveChangesAsync();
        var repo = new GameHistoryRepository(context);

        var (records, _) = await repo.GetHistoryAsync("user1", page: 1, pageSize: 3);

        Assert.Equal(3, records.Count);
        Assert.True(records[0].StartedAt > records[1].StartedAt);
        Assert.True(records[1].StartedAt > records[2].StartedAt);
    }

    [Fact]
    public async Task GetHistoryAsync_ReturnsCorrectTotalCount()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var context = await CreateContextWithUser(dbName);
        var baseDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        for (int i = 0; i < 5; i++)
        {
            context.GameRecords.Add(CreateGameRecord("user1", GameResult.Win, 100m, 200m, baseDate.AddDays(i)));
        }
        await context.SaveChangesAsync();
        var repo = new GameHistoryRepository(context);

        var (_, totalCount) = await repo.GetHistoryAsync("user1", page: 1, pageSize: 3);

        Assert.Equal(5, totalCount);
    }

    [Fact]
    public async Task GetHistoryAsync_Page2ReturnsCorrectRecords()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var context = await CreateContextWithUser(dbName);
        var baseDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        for (int i = 0; i < 5; i++)
        {
            context.GameRecords.Add(CreateGameRecord("user1", GameResult.Win, 100m, 200m, baseDate.AddDays(i)));
        }
        await context.SaveChangesAsync();
        var repo = new GameHistoryRepository(context);

        var (records, _) = await repo.GetHistoryAsync("user1", page: 2, pageSize: 3);

        Assert.Equal(2, records.Count);
    }

    [Fact]
    public async Task GetStatisticsAsync_ReturnsCorrectCountsForEachResultType()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var context = await CreateContextWithUser(dbName);
        var baseDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        context.GameRecords.Add(CreateGameRecord("user1", GameResult.Win, 100m, 200m, baseDate));
        context.GameRecords.Add(CreateGameRecord("user1", GameResult.Win, 100m, 200m, baseDate.AddDays(1)));
        context.GameRecords.Add(CreateGameRecord("user1", GameResult.Lose, 100m, 0m, baseDate.AddDays(2)));
        context.GameRecords.Add(CreateGameRecord("user1", GameResult.Push, 100m, 100m, baseDate.AddDays(3)));
        context.GameRecords.Add(CreateGameRecord("user1", GameResult.Blackjack, 100m, 250m, baseDate.AddDays(4)));
        await context.SaveChangesAsync();
        var repo = new GameHistoryRepository(context);

        var stats = await repo.GetStatisticsAsync("user1");

        Assert.Equal(5, stats.TotalGames);
        Assert.Equal(2, stats.Wins);
        Assert.Equal(1, stats.Losses);
        Assert.Equal(1, stats.Pushes);
        Assert.Equal(1, stats.Blackjacks);
    }

    [Fact]
    public async Task GetAllHistoryAsync_ReturnsAllRecordsOrderedByDateDesc()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var context = await CreateContextWithUser(dbName);
        var baseDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        for (int i = 0; i < 20; i++)
        {
            context.GameRecords.Add(CreateGameRecord("user1", GameResult.Win, 100m, 200m, baseDate.AddDays(i)));
        }
        await context.SaveChangesAsync();
        var repo = new GameHistoryRepository(context);

        var records = await repo.GetAllHistoryAsync("user1");

        Assert.Equal(20, records.Count);
        for (int i = 0; i < records.Count - 1; i++)
        {
            Assert.True(records[i].StartedAt >= records[i + 1].StartedAt);
        }
    }

    [Fact]
    public async Task GetAllHistoryAsync_ReturnsEmptyListForUserWithNoGames()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var context = await CreateContextWithUser(dbName);
        var repo = new GameHistoryRepository(context);

        var records = await repo.GetAllHistoryAsync("user1");

        Assert.Empty(records);
    }

    [Fact]
    public async Task GetAllHistoryAsync_ReturnsOnlyRecordsForSpecifiedUser()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var context = CreateContext(dbName);
        context.Users.Add(new ApplicationUser { Id = "user1", UserName = "a@a.com", Email = "a@a.com", Balance = 1000m });
        context.Users.Add(new ApplicationUser { Id = "user2", UserName = "b@b.com", Email = "b@b.com", Balance = 1000m });
        await context.SaveChangesAsync();
        var baseDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        context.GameRecords.Add(CreateGameRecord("user1", GameResult.Win, 100m, 200m, baseDate));
        context.GameRecords.Add(CreateGameRecord("user1", GameResult.Lose, 50m, 0m, baseDate.AddDays(1)));
        context.GameRecords.Add(CreateGameRecord("user2", GameResult.Win, 75m, 150m, baseDate.AddDays(2)));
        await context.SaveChangesAsync();
        var repo = new GameHistoryRepository(context);

        var records = await repo.GetAllHistoryAsync("user1");

        Assert.Equal(2, records.Count);
        Assert.All(records, r => Assert.Equal("user1", r.UserId));
    }

    [Fact]
    public async Task GetStatisticsAsync_ReturnsZeroStatsForUserWithNoGames()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var context = await CreateContextWithUser(dbName);
        var repo = new GameHistoryRepository(context);

        var stats = await repo.GetStatisticsAsync("user1");

        Assert.Equal(0, stats.TotalGames);
        Assert.Equal(0, stats.Wins);
        Assert.Equal(0, stats.Losses);
        Assert.Equal(0, stats.Pushes);
        Assert.Equal(0, stats.Blackjacks);
        Assert.Equal(0m, stats.BiggestWin);
        Assert.Equal(0m, stats.TotalWagered);
        Assert.Equal(0m, stats.NetProfitLoss);
        Assert.Equal(0d, stats.WinRate);
    }

    [Fact]
    public async Task GetStatisticsAsync_CalculatesBiggestWinTotalWageredAndNetProfitLoss()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var context = await CreateContextWithUser(dbName);
        var baseDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        context.GameRecords.Add(CreateGameRecord("user1", GameResult.Win, 50m, 100m, baseDate));
        context.GameRecords.Add(CreateGameRecord("user1", GameResult.Blackjack, 100m, 250m, baseDate.AddDays(1)));
        context.GameRecords.Add(CreateGameRecord("user1", GameResult.Lose, 75m, 0m, baseDate.AddDays(2)));
        await context.SaveChangesAsync();
        var repo = new GameHistoryRepository(context);

        var stats = await repo.GetStatisticsAsync("user1");

        Assert.Equal(250m, stats.BiggestWin);      // max FinalPayout
        Assert.Equal(225m, stats.TotalWagered);     // 50 + 100 + 75
        Assert.Equal(125m, stats.NetProfitLoss);    // (100-50) + (250-100) + (0-75) = 50 + 150 - 75 = 125
    }
}
