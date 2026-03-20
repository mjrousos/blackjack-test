namespace Blackjack.Infrastructure.Repositories;

using Blackjack.Domain.Models;
using Blackjack.Domain.Services;
using Blackjack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public class AchievementRepository(BlackjackDbContext dbContext) : IAchievementRepository
{
    public async Task<List<string>> GetEarnedAchievementIdsAsync(string userId)
    {
        return await dbContext.UserAchievements
            .Where(a => a.UserId == userId)
            .Select(a => a.AchievementId)
            .ToListAsync();
    }

    public async Task<Dictionary<string, DateTime>> GetEarnedAchievementsWithDatesAsync(string userId)
    {
        return await dbContext.UserAchievements
            .Where(a => a.UserId == userId)
            .ToDictionaryAsync(a => a.AchievementId, a => a.EarnedAt);
    }

    public async Task<List<string>> CheckAndAwardNewAchievementsAsync(string userId)
    {
        var stats = await dbContext.GameRecords
            .Where(g => g.UserId == userId)
            .GroupBy(g => g.UserId)
            .Select(grp => new
            {
                TotalGames = grp.Count(),
                Wins = grp.Count(g => g.Result == GameResult.Win),
                Blackjacks = grp.Count(g => g.Result == GameResult.Blackjack),
                TotalWagered = grp.Sum(g => g.InitialBet),
                NetProfitLoss = grp.Sum(g => g.FinalPayout - g.InitialBet)
            })
            .FirstOrDefaultAsync();

        if (stats == null)
            return [];

        var recentResults = await dbContext.GameRecords
            .Where(g => g.UserId == userId)
            .OrderByDescending(g => g.StartedAt)
            .Take(5)
            .Select(g => g.Result)
            .ToListAsync();

        var earnedIds = AchievementEvaluator.GetEarnedAchievementIds(
            stats.TotalGames,
            stats.Wins,
            stats.Blackjacks,
            stats.TotalWagered,
            stats.NetProfitLoss,
            recentResults).ToHashSet();

        var alreadyEarnedIds = await dbContext.UserAchievements
            .Where(a => a.UserId == userId)
            .Select(a => a.AchievementId)
            .ToListAsync();

        var newIds = earnedIds.Except(alreadyEarnedIds).ToList();

        if (newIds.Count > 0)
        {
            foreach (var id in newIds)
            {
                dbContext.UserAchievements.Add(new UserAchievement
                {
                    UserId = userId,
                    AchievementId = id,
                    EarnedAt = DateTime.UtcNow
                });
            }
            await dbContext.SaveChangesAsync();
        }

        return newIds;
    }
}
