namespace Blackjack.Infrastructure.Repositories;

using Blackjack.Domain.Models;
using Blackjack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public class GameHistoryRepository(BlackjackDbContext dbContext) : IGameHistoryRepository
{
    public async Task SaveGameRecordAsync(GameRecord record)
    {
        dbContext.GameRecords.Add(record);
        await dbContext.SaveChangesAsync();
    }

    public async Task<(List<GameRecord> Records, int TotalCount)> GetHistoryAsync(string userId, int page = 1, int pageSize = 10)
    {
        var query = dbContext.GameRecords
            .Where(g => g.UserId == userId)
            .OrderByDescending(g => g.StartedAt);

        var totalCount = await query.CountAsync();
        var records = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (records, totalCount);
    }

    public async Task<List<GameRecord>> GetAllHistoryAsync(string userId)
    {
        return await dbContext.GameRecords
            .Where(g => g.UserId == userId)
            .OrderByDescending(g => g.StartedAt)
            .ToListAsync();
    }

    public async Task<PlayerStatistics> GetStatisticsAsync(string userId)
    {
        var games = dbContext.GameRecords.Where(g => g.UserId == userId);

        var totalGames = await games.CountAsync();

        if (totalGames == 0)
        {
            return new PlayerStatistics();
        }

        return new PlayerStatistics
        {
            TotalGames = totalGames,
            Wins = await games.CountAsync(g => g.Result == GameResult.Win),
            Losses = await games.CountAsync(g => g.Result == GameResult.Lose),
            Pushes = await games.CountAsync(g => g.Result == GameResult.Push),
            Blackjacks = await games.CountAsync(g => g.Result == GameResult.Blackjack),
            BiggestWin = await games.MaxAsync(g => g.FinalPayout),
            TotalWagered = await games.SumAsync(g => g.InitialBet),
            NetProfitLoss = await games.SumAsync(g => g.FinalPayout - g.InitialBet)
        };
    }
}
