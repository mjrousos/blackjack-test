namespace Blackjack.Domain.Services;

using Blackjack.Domain.Models;

public static class AchievementEvaluator
{
    public static readonly IReadOnlyList<Achievement> AllAchievements =
    [
        new("first_win",     "First Win",     "Win your first game",                           "🎯"),
        new("natural",       "Natural",       "Get your first blackjack",                      "🃏"),
        new("high_roller",   "High Roller",   "Wager a total of $5,000",                       "💰"),
        new("century_club",  "Century Club",  "Play 100 games",                                "🏆"),
        new("in_the_black",  "In the Black",  "Achieve a positive net profit over 10+ games",  "📈"),
        new("hot_streak",    "Hot Streak",    "Win 5 consecutive games",                       "🔥"),
    ];

    /// <summary>
    /// Returns the IDs of all achievements that have been earned based on the player's statistics
    /// and their most recent game results (ordered newest-first).
    /// </summary>
    /// <param name="totalGames">Total number of games played.</param>
    /// <param name="wins">Number of wins.</param>
    /// <param name="blackjacks">Number of blackjacks.</param>
    /// <param name="totalWagered">Cumulative amount wagered.</param>
    /// <param name="netProfitLoss">Net profit/loss across all games.</param>
    /// <param name="recentResultsNewestFirst">Recent game results ordered newest-first, used for streak detection.</param>
    public static IEnumerable<string> GetEarnedAchievementIds(
        int totalGames,
        int wins,
        int blackjacks,
        decimal totalWagered,
        decimal netProfitLoss,
        IReadOnlyList<GameResult> recentResultsNewestFirst)
    {
        if (wins >= 1 || blackjacks >= 1)
            yield return "first_win";

        if (blackjacks >= 1)
            yield return "natural";

        if (totalWagered >= 5_000m)
            yield return "high_roller";

        if (totalGames >= 100)
            yield return "century_club";

        if (totalGames >= 10 && netProfitLoss > 0)
            yield return "in_the_black";

        if (HasConsecutiveWins(recentResultsNewestFirst, 5))
            yield return "hot_streak";
    }

    private static bool HasConsecutiveWins(IReadOnlyList<GameResult> resultsNewestFirst, int count)
    {
        if (resultsNewestFirst.Count < count)
            return false;

        int consecutive = 0;
        foreach (var result in resultsNewestFirst)
        {
            if (result == GameResult.Win || result == GameResult.Blackjack)
            {
                consecutive++;
                if (consecutive >= count)
                    return true;
            }
            else
            {
                break;
            }
        }
        return false;
    }
}
