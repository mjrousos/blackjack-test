namespace Blackjack.Infrastructure.Repositories;

public interface IAchievementRepository
{
    Task<List<string>> GetEarnedAchievementIdsAsync(string userId);
    Task<Dictionary<string, DateTime>> GetEarnedAchievementsWithDatesAsync(string userId);
    Task<List<string>> CheckAndAwardNewAchievementsAsync(string userId);
}
