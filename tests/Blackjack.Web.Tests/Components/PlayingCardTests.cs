using Bunit;
using Xunit;
using Blackjack.Domain.Models;
using Blackjack.Web.Components;

namespace Blackjack.Web.Tests.Components;

public class PlayingCardTests : BunitContext
{
    [Fact]
    public void FaceDown_ShowsCardBack_NotCardFace()
    {
        var cut = Render<PlayingCard>(p => p
            .Add(x => x.Card, new Card(Suit.Hearts, Rank.Ace))
            .Add(x => x.FaceDown, true));

        cut.Find(".card-back");
        Assert.Throws<ElementNotFoundException>(() => cut.Find(".card-face"));
    }

    [Fact]
    public void FaceUp_WithCard_ShowsCardFace()
    {
        var cut = Render<PlayingCard>(p => p
            .Add(x => x.Card, new Card(Suit.Spades, Rank.King))
            .Add(x => x.FaceDown, false));

        cut.Find(".card-face");
        Assert.Throws<ElementNotFoundException>(() => cut.Find(".card-back"));
    }

    [Theory]
    [InlineData(Suit.Hearts)]
    [InlineData(Suit.Diamonds)]
    public void RedSuits_HaveRedSuitClass(Suit suit)
    {
        var cut = Render<PlayingCard>(p => p
            .Add(x => x.Card, new Card(suit, Rank.Five))
            .Add(x => x.FaceDown, false));

        cut.Find(".red-suit");
    }

    [Theory]
    [InlineData(Suit.Clubs)]
    [InlineData(Suit.Spades)]
    public void BlackSuits_HaveBlackSuitClass(Suit suit)
    {
        var cut = Render<PlayingCard>(p => p
            .Add(x => x.Card, new Card(suit, Rank.Five))
            .Add(x => x.FaceDown, false));

        cut.Find(".black-suit");
    }

    [Theory]
    [InlineData(Rank.Ace, "A")]
    [InlineData(Rank.Jack, "J")]
    [InlineData(Rank.Queen, "Q")]
    [InlineData(Rank.King, "K")]
    [InlineData(Rank.Seven, "7")]
    [InlineData(Rank.Ten, "10")]
    public void RankDisplay_ShowsCorrectText(Rank rank, string expected)
    {
        var cut = Render<PlayingCard>(p => p
            .Add(x => x.Card, new Card(Suit.Spades, rank))
            .Add(x => x.FaceDown, false));

        var rankEl = cut.Find(".card-rank");
        Assert.Equal(expected, rankEl.TextContent);
    }

    [Theory]
    [InlineData(Suit.Hearts, "♥")]
    [InlineData(Suit.Diamonds, "♦")]
    [InlineData(Suit.Clubs, "♣")]
    [InlineData(Suit.Spades, "♠")]
    public void SuitSymbol_ShowsCorrectSymbol(Suit suit, string expected)
    {
        var cut = Render<PlayingCard>(p => p
            .Add(x => x.Card, new Card(suit, Rank.Ace))
            .Add(x => x.FaceDown, false));

        var suitEl = cut.Find(".card-suit-large");
        Assert.Equal(expected, suitEl.TextContent);
    }

    [Fact]
    public void IsNew_AddsDealAnimationClass()
    {
        var cut = Render<PlayingCard>(p => p
            .Add(x => x.Card, new Card(Suit.Hearts, Rank.Ace))
            .Add(x => x.IsNew, true));

        cut.Find(".deal-animation");
    }

    [Fact]
    public void IsNew_False_NoDealAnimationClass()
    {
        var cut = Render<PlayingCard>(p => p
            .Add(x => x.Card, new Card(Suit.Hearts, Rank.Ace))
            .Add(x => x.IsNew, false));

        var root = cut.Find(".playing-card");
        Assert.DoesNotContain("deal-animation", root.ClassList);
    }
}
