namespace Blackjack.Infrastructure.Repositories;

public interface IPlayerRepository
{
    Task<decimal> GetBalanceAsync(string userId);
    Task UpdateBalanceAsync(string userId, decimal amount);
    Task RefillBalanceAsync(string userId, decimal startingBalance);
    Task UpdateLastLoginAsync(string userId);
}
