namespace Blackjack.Infrastructure.Tests.Repositories;

using Blackjack.Infrastructure.Data;
using Blackjack.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class PlayerRepositoryTests
{
    private static BlackjackDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<BlackjackDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new BlackjackDbContext(options);
    }

    private static async Task<BlackjackDbContext> CreateContextWithUser(string dbName, string userId = "user1", decimal balance = 1000m)
    {
        var context = CreateContext(dbName);
        var user = new ApplicationUser
        {
            Id = userId,
            UserName = "test@test.com",
            Email = "test@test.com",
            Balance = balance
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return context;
    }

    [Fact]
    public async Task GetBalanceAsync_ReturnsCorrectBalance()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var context = await CreateContextWithUser(dbName, balance: 500m);
        var repo = new PlayerRepository(context);

        var balance = await repo.GetBalanceAsync("user1");

        Assert.Equal(500m, balance);
    }

    [Fact]
    public async Task GetBalanceAsync_ThrowsForUnknownUser()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var context = CreateContext(dbName);
        var repo = new PlayerRepository(context);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => repo.GetBalanceAsync("nonexistent"));
    }

    [Fact]
    public async Task UpdateBalanceAsync_SetsNewBalance()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var context = await CreateContextWithUser(dbName, balance: 1000m);
        var repo = new PlayerRepository(context);

        await repo.UpdateBalanceAsync("user1", 750m);

        var user = await context.Users.FindAsync("user1");
        Assert.Equal(750m, user!.Balance);
    }

    [Fact]
    public async Task RefillBalanceAsync_RefillsWhenBalanceIsZero()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var context = await CreateContextWithUser(dbName, balance: 0m);
        var repo = new PlayerRepository(context);

        await repo.RefillBalanceAsync("user1", 1000m);

        var user = await context.Users.FindAsync("user1");
        Assert.Equal(1000m, user!.Balance);
    }

    [Fact]
    public async Task RefillBalanceAsync_DoesNotRefillWhenBalanceIsPositive()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var context = await CreateContextWithUser(dbName, balance: 500m);
        var repo = new PlayerRepository(context);

        await repo.RefillBalanceAsync("user1", 1000m);

        var user = await context.Users.FindAsync("user1");
        Assert.Equal(500m, user!.Balance);
    }

    [Fact]
    public async Task UpdateLastLoginAsync_SetsTimestamp()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var context = await CreateContextWithUser(dbName);
        var repo = new PlayerRepository(context);
        var before = DateTime.UtcNow;

        await repo.UpdateLastLoginAsync("user1");

        var user = await context.Users.FindAsync("user1");
        Assert.NotNull(user!.LastLoginAt);
        Assert.InRange(user.LastLoginAt!.Value, before, DateTime.UtcNow.AddSeconds(1));
    }
}
