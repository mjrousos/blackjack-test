namespace Blackjack.Infrastructure.Repositories;

using Blackjack.Infrastructure.Data;

public interface IGameHistoryRepository
{
    Task SaveGameRecordAsync(GameRecord record);
    Task<(List<GameRecord> Records, int TotalCount)> GetHistoryAsync(string userId, int page = 1, int pageSize = 10);
    Task<List<GameRecord>> GetAllHistoryAsync(string userId);
    Task<PlayerStatistics> GetStatisticsAsync(string userId);
}
