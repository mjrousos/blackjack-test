namespace Blackjack.Domain.Tests.Services;

using Blackjack.Domain.Models;
using Blackjack.Domain.Services;
using FluentAssertions;
using Xunit;

public class AchievementEvaluatorTests
{
    private static IEnumerable<string> Evaluate(
        int totalGames = 0,
        int wins = 0,
        int blackjacks = 0,
        decimal totalWagered = 0m,
        decimal netProfitLoss = 0m,
        IReadOnlyList<GameResult>? recentResults = null)
        => AchievementEvaluator.GetEarnedAchievementIds(
            totalGames, wins, blackjacks, totalWagered, netProfitLoss,
            recentResults ?? []);

    [Fact]
    public void AllAchievements_ContainsSixEntries()
    {
        AchievementEvaluator.AllAchievements.Should().HaveCount(6);
    }

    [Fact]
    public void NoAchievements_WhenNoGamesPlayed()
    {
        var ids = Evaluate();
        ids.Should().BeEmpty();
    }

    [Fact]
    public void FirstWin_EarnedWhenWinsGreaterThanZero()
    {
        var ids = Evaluate(totalGames: 1, wins: 1);
        ids.Should().Contain("first_win");
    }

    [Fact]
    public void FirstWin_EarnedWhenBlackjackGreaterThanZero()
    {
        var ids = Evaluate(totalGames: 1, blackjacks: 1);
        ids.Should().Contain("first_win");
    }

    [Fact]
    public void FirstWin_NotEarnedWithOnlyLosses()
    {
        var ids = Evaluate(totalGames: 5, wins: 0, blackjacks: 0);
        ids.Should().NotContain("first_win");
    }

    [Fact]
    public void Natural_EarnedWhenBlackjacksGreaterThanZero()
    {
        var ids = Evaluate(totalGames: 1, blackjacks: 1);
        ids.Should().Contain("natural");
    }

    [Fact]
    public void Natural_NotEarnedWithOnlyRegularWins()
    {
        var ids = Evaluate(totalGames: 5, wins: 5, blackjacks: 0);
        ids.Should().NotContain("natural");
    }

    [Fact]
    public void HighRoller_EarnedWhenTotalWageredIsAtLeast5000()
    {
        var ids = Evaluate(totalGames: 50, totalWagered: 5000m);
        ids.Should().Contain("high_roller");
    }

    [Fact]
    public void HighRoller_EarnedWhenTotalWageredExceeds5000()
    {
        var ids = Evaluate(totalGames: 100, totalWagered: 10000m);
        ids.Should().Contain("high_roller");
    }

    [Fact]
    public void HighRoller_NotEarnedBelow5000()
    {
        var ids = Evaluate(totalGames: 10, totalWagered: 4999m);
        ids.Should().NotContain("high_roller");
    }

    [Fact]
    public void CenturyClub_EarnedWhenTotalGamesReaches100()
    {
        var ids = Evaluate(totalGames: 100);
        ids.Should().Contain("century_club");
    }

    [Fact]
    public void CenturyClub_EarnedWithMoreThan100Games()
    {
        var ids = Evaluate(totalGames: 150);
        ids.Should().Contain("century_club");
    }

    [Fact]
    public void CenturyClub_NotEarnedBelow100Games()
    {
        var ids = Evaluate(totalGames: 99);
        ids.Should().NotContain("century_club");
    }

    [Fact]
    public void InTheBlack_EarnedWithPositiveProfitAndAtLeast10Games()
    {
        var ids = Evaluate(totalGames: 10, netProfitLoss: 1m);
        ids.Should().Contain("in_the_black");
    }

    [Fact]
    public void InTheBlack_NotEarnedWithNegativeProfit()
    {
        var ids = Evaluate(totalGames: 20, netProfitLoss: -1m);
        ids.Should().NotContain("in_the_black");
    }

    [Fact]
    public void InTheBlack_NotEarnedWithFewerThan10Games()
    {
        var ids = Evaluate(totalGames: 9, netProfitLoss: 500m);
        ids.Should().NotContain("in_the_black");
    }

    [Fact]
    public void InTheBlack_NotEarnedWithZeroProfit()
    {
        var ids = Evaluate(totalGames: 20, netProfitLoss: 0m);
        ids.Should().NotContain("in_the_black");
    }

    [Fact]
    public void HotStreak_EarnedWithFiveConsecutiveWins()
    {
        IReadOnlyList<GameResult> results = [
            GameResult.Win, GameResult.Win, GameResult.Win, GameResult.Win, GameResult.Win
        ];
        var ids = Evaluate(totalGames: 5, wins: 5, recentResults: results);
        ids.Should().Contain("hot_streak");
    }

    [Fact]
    public void HotStreak_EarnedWithFiveConsecutiveBlackjacks()
    {
        IReadOnlyList<GameResult> results = [
            GameResult.Blackjack, GameResult.Blackjack, GameResult.Blackjack,
            GameResult.Blackjack, GameResult.Blackjack
        ];
        var ids = Evaluate(totalGames: 5, blackjacks: 5, recentResults: results);
        ids.Should().Contain("hot_streak");
    }

    [Fact]
    public void HotStreak_EarnedWithMixedWinsAndBlackjacks()
    {
        IReadOnlyList<GameResult> results = [
            GameResult.Win, GameResult.Blackjack, GameResult.Win, GameResult.Win, GameResult.Blackjack
        ];
        var ids = Evaluate(totalGames: 5, recentResults: results);
        ids.Should().Contain("hot_streak");
    }

    [Fact]
    public void HotStreak_NotEarnedWithLossBreakingStreak()
    {
        IReadOnlyList<GameResult> results = [
            GameResult.Win, GameResult.Win, GameResult.Lose, GameResult.Win, GameResult.Win
        ];
        var ids = Evaluate(totalGames: 5, wins: 4, recentResults: results);
        ids.Should().NotContain("hot_streak");
    }

    [Fact]
    public void HotStreak_NotEarnedWithFewerThanFiveRecentResults()
    {
        IReadOnlyList<GameResult> results = [
            GameResult.Win, GameResult.Win, GameResult.Win, GameResult.Win
        ];
        var ids = Evaluate(totalGames: 4, wins: 4, recentResults: results);
        ids.Should().NotContain("hot_streak");
    }

    [Fact]
    public void MultipleAchievements_CanBeEarnedSimultaneously()
    {
        IReadOnlyList<GameResult> results = [
            GameResult.Win, GameResult.Win, GameResult.Win, GameResult.Win, GameResult.Win
        ];
        var ids = Evaluate(
            totalGames: 100,
            wins: 60,
            blackjacks: 5,
            totalWagered: 10000m,
            netProfitLoss: 500m,
            recentResults: results).ToList();

        ids.Should().Contain("first_win");
        ids.Should().Contain("natural");
        ids.Should().Contain("high_roller");
        ids.Should().Contain("century_club");
        ids.Should().Contain("in_the_black");
        ids.Should().Contain("hot_streak");
    }
}
