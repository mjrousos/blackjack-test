namespace Blackjack.Infrastructure.Repositories;

using Blackjack.Infrastructure.Data;

public class PlayerRepository(BlackjackDbContext dbContext) : IPlayerRepository
{
    public async Task<decimal> GetBalanceAsync(string userId)
    {
        var user = await dbContext.Users.FindAsync(userId)
            ?? throw new InvalidOperationException($"User '{userId}' not found.");
        return user.Balance;
    }

    public async Task UpdateBalanceAsync(string userId, decimal amount)
    {
        var user = await dbContext.Users.FindAsync(userId)
            ?? throw new InvalidOperationException($"User '{userId}' not found.");
        user.Balance = amount;
        await dbContext.SaveChangesAsync();
    }

    public async Task RefillBalanceAsync(string userId, decimal startingBalance)
    {
        var user = await dbContext.Users.FindAsync(userId)
            ?? throw new InvalidOperationException($"User '{userId}' not found.");
        if (user.Balance <= 0)
        {
            user.Balance = startingBalance;
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task UpdateLastLoginAsync(string userId)
    {
        var user = await dbContext.Users.FindAsync(userId)
            ?? throw new InvalidOperationException($"User '{userId}' not found.");
        user.LastLoginAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();
    }
}
