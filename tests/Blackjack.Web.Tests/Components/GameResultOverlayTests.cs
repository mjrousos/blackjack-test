using Bunit;
using Xunit;
using Blackjack.Domain.Models;
using Blackjack.Web.Components;

namespace Blackjack.Web.Tests.Components;

public class GameResultOverlayTests : BunitContext
{
    [Fact]
    public void IsVisible_True_AddsVisibleClass()
    {
        var cut = Render<GameResultOverlay>(p => p
            .Add(x => x.Result, GameResult.Win)
            .Add(x => x.IsVisible, true));

        var overlay = cut.Find(".result-overlay");
        Assert.Contains("visible", overlay.ClassList);
    }

    [Fact]
    public void IsVisible_False_NoVisibleClass()
    {
        var cut = Render<GameResultOverlay>(p => p
            .Add(x => x.Result, GameResult.Win)
            .Add(x => x.IsVisible, false));

        var overlay = cut.Find(".result-overlay");
        Assert.DoesNotContain("visible", overlay.ClassList);
    }

    [Theory]
    [InlineData(GameResult.Win, "You Win!", "result-win")]
    [InlineData(GameResult.Blackjack, "Blackjack!", "result-blackjack")]
    [InlineData(GameResult.Push, "Push", "result-push")]
    [InlineData(GameResult.Lose, "Dealer Wins", "result-lose")]
    public void ResultType_ShowsCorrectTitleAndClass(GameResult result, string expectedTitle, string expectedClass)
    {
        var cut = Render<GameResultOverlay>(p => p
            .Add(x => x.Result, result)
            .Add(x => x.IsVisible, true));

        var title = cut.Find(".result-title");
        Assert.Equal(expectedTitle, title.TextContent);

        var card = cut.Find(".result-card");
        Assert.Contains(expectedClass, card.ClassList);
    }

    [Fact]
    public void Win_PayoutText_ShowsProfit()
    {
        var cut = Render<GameResultOverlay>(p => p
            .Add(x => x.Result, GameResult.Win)
            .Add(x => x.Bet, 25m)
            .Add(x => x.Payout, 50m)
            .Add(x => x.IsVisible, true));

        var payout = cut.Find(".result-payout");
        Assert.Equal("+$25", payout.TextContent);
    }

    [Fact]
    public void Blackjack_PayoutText_ShowsProfit()
    {
        var cut = Render<GameResultOverlay>(p => p
            .Add(x => x.Result, GameResult.Blackjack)
            .Add(x => x.Bet, 20m)
            .Add(x => x.Payout, 50m)
            .Add(x => x.IsVisible, true));

        var payout = cut.Find(".result-payout");
        Assert.Equal("+$30", payout.TextContent);
    }

    [Fact]
    public void Lose_PayoutText_ShowsNegativeBet()
    {
        var cut = Render<GameResultOverlay>(p => p
            .Add(x => x.Result, GameResult.Lose)
            .Add(x => x.Bet, 25m)
            .Add(x => x.Payout, 0m)
            .Add(x => x.IsVisible, true));

        var payout = cut.Find(".result-payout");
        Assert.Equal("-$25", payout.TextContent);
    }

    [Fact]
    public void Push_PayoutText_ShowsBetReturned()
    {
        var cut = Render<GameResultOverlay>(p => p
            .Add(x => x.Result, GameResult.Push)
            .Add(x => x.Bet, 25m)
            .Add(x => x.Payout, 25m)
            .Add(x => x.IsVisible, true));

        var payout = cut.Find(".result-payout");
        Assert.Equal("Bet returned", payout.TextContent);
    }

    [Fact]
    public void SplitResults_ShowsBothHandResults()
    {
        var cut = Render<GameResultOverlay>(p => p
            .Add(x => x.Result, GameResult.Win)
            .Add(x => x.Payout, 30m)
            .Add(x => x.Bet, 10m)
            .Add(x => x.IsVisible, true)
            .Add(x => x.SplitResults, new List<GameResult?> { GameResult.Win, GameResult.Lose })
            .Add(x => x.SplitBets, new List<decimal> { 10m, 10m }));

        var handResults = cut.FindAll(".split-hand-result");
        Assert.Equal(2, handResults.Count);
    }

    [Fact]
    public void SplitResults_ShowsCorrectHandLabels()
    {
        var cut = Render<GameResultOverlay>(p => p
            .Add(x => x.Result, GameResult.Win)
            .Add(x => x.Payout, 30m)
            .Add(x => x.Bet, 10m)
            .Add(x => x.IsVisible, true)
            .Add(x => x.SplitResults, new List<GameResult?> { GameResult.Win, GameResult.Lose })
            .Add(x => x.SplitBets, new List<decimal> { 10m, 10m }));

        var labels = cut.FindAll(".split-hand-label");
        Assert.Equal("Hand 1", labels[0].TextContent);
        Assert.Equal("Hand 2", labels[1].TextContent);
    }

    [Fact]
    public void SplitResults_MixedResults_ShowsSplitResultsTitle()
    {
        var cut = Render<GameResultOverlay>(p => p
            .Add(x => x.Result, GameResult.Win)
            .Add(x => x.Payout, 20m)
            .Add(x => x.Bet, 10m)
            .Add(x => x.IsVisible, true)
            .Add(x => x.SplitResults, new List<GameResult?> { GameResult.Win, GameResult.Lose })
            .Add(x => x.SplitBets, new List<decimal> { 10m, 10m }));

        var title = cut.Find(".result-title");
        Assert.Equal("Split Results", title.TextContent);
    }

    [Fact]
    public void SplitResults_BothWin_ShowsBothHandsWinTitle()
    {
        var cut = Render<GameResultOverlay>(p => p
            .Add(x => x.Result, GameResult.Win)
            .Add(x => x.Payout, 40m)
            .Add(x => x.Bet, 10m)
            .Add(x => x.IsVisible, true)
            .Add(x => x.SplitResults, new List<GameResult?> { GameResult.Win, GameResult.Win })
            .Add(x => x.SplitBets, new List<decimal> { 10m, 10m }));

        var title = cut.Find(".result-title");
        Assert.Equal("Both Hands Win!", title.TextContent);
    }

    [Fact]
    public void SplitResults_ShowsTotalPayout()
    {
        var cut = Render<GameResultOverlay>(p => p
            .Add(x => x.Result, GameResult.Win)
            .Add(x => x.Payout, 20m)
            .Add(x => x.Bet, 10m)
            .Add(x => x.IsVisible, true)
            .Add(x => x.SplitResults, new List<GameResult?> { GameResult.Win, GameResult.Lose })
            .Add(x => x.SplitBets, new List<decimal> { 10m, 10m }));

        var totalPayout = cut.Find(".total-payout");
        Assert.Contains("Total:", totalPayout.TextContent);
    }

    [Fact]
    public void SingleResult_DoesNotShowSplitLayout()
    {
        var cut = Render<GameResultOverlay>(p => p
            .Add(x => x.Result, GameResult.Win)
            .Add(x => x.Payout, 20m)
            .Add(x => x.Bet, 10m)
            .Add(x => x.IsVisible, true)
            .Add(x => x.SplitResults, new List<GameResult?> { GameResult.Win })
            .Add(x => x.SplitBets, new List<decimal> { 10m }));

        var splitResults = cut.FindAll(".split-hand-result");
        Assert.Empty(splitResults);

        var title = cut.Find(".result-title");
        Assert.Equal("You Win!", title.TextContent);
    }

    [Fact]
    public void NullSplitResults_DoesNotShowSplitLayout()
    {
        var cut = Render<GameResultOverlay>(p => p
            .Add(x => x.Result, GameResult.Win)
            .Add(x => x.Payout, 20m)
            .Add(x => x.Bet, 10m)
            .Add(x => x.IsVisible, true));

        var splitResults = cut.FindAll(".split-hand-result");
        Assert.Empty(splitResults);
    }

    [Fact]
    public void SplitResults_WinAndPush_ShowsSplitResultsTitle()
    {
        var cut = Render<GameResultOverlay>(p => p
            .Add(x => x.Result, GameResult.Win)
            .Add(x => x.Payout, 30m)
            .Add(x => x.Bet, 10m)
            .Add(x => x.IsVisible, true)
            .Add(x => x.SplitResults, new List<GameResult?> { GameResult.Win, GameResult.Push })
            .Add(x => x.SplitBets, new List<decimal> { 10m, 10m }));

        var title = cut.Find(".result-title");
        Assert.Equal("Split Results", title.TextContent);
    }

    [Fact]
    public void SplitResults_LoseAndPush_ShowsSplitResultsTitle()
    {
        var cut = Render<GameResultOverlay>(p => p
            .Add(x => x.Result, GameResult.Lose)
            .Add(x => x.Payout, 10m)
            .Add(x => x.Bet, 10m)
            .Add(x => x.IsVisible, true)
            .Add(x => x.SplitResults, new List<GameResult?> { GameResult.Lose, GameResult.Push })
            .Add(x => x.SplitBets, new List<decimal> { 10m, 10m }));

        var title = cut.Find(".result-title");
        Assert.Equal("Split Results", title.TextContent);
    }
}
