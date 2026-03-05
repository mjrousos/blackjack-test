using Bunit;
using Xunit;
using Blackjack.Domain.Models;
using Blackjack.Web.Components;

namespace Blackjack.Web.Tests.Components;

public class CardHandTests : BunitContext
{
    private static Hand CreateHand(params Card[] cards)
    {
        var hand = new Hand();
        foreach (var card in cards)
            hand.AddCard(card);
        return hand;
    }

    [Fact]
    public void RendersLabelInHandTitle()
    {
        var hand = CreateHand(new Card(Suit.Hearts, Rank.Five));

        var cut = Render<CardHand>(p => p
            .Add(x => x.Hand, hand)
            .Add(x => x.Label, "Player"));

        var title = cut.Find(".hand-title");
        Assert.Equal("Player", title.TextContent);
    }

    [Fact]
    public void ShowScore_True_DisplaysScoreElement()
    {
        var hand = CreateHand(
            new Card(Suit.Hearts, Rank.Five),
            new Card(Suit.Clubs, Rank.Eight));

        var cut = Render<CardHand>(p => p
            .Add(x => x.Hand, hand)
            .Add(x => x.Label, "Player")
            .Add(x => x.ShowScore, true));

        var score = cut.Find(".hand-score");
        Assert.Equal("13", score.TextContent);
    }

    [Fact]
    public void ShowScore_False_NoScoreElement()
    {
        var hand = CreateHand(new Card(Suit.Hearts, Rank.Five));

        var cut = Render<CardHand>(p => p
            .Add(x => x.Hand, hand)
            .Add(x => x.Label, "Player")
            .Add(x => x.ShowScore, false));

        Assert.Throws<ElementNotFoundException>(() => cut.Find(".hand-score"));
    }

    [Fact]
    public void Dealer_HideSecondCard_ScoreShowsFirstCardOnly()
    {
        var hand = CreateHand(
            new Card(Suit.Hearts, Rank.Ten),
            new Card(Suit.Clubs, Rank.Seven));

        var cut = Render<CardHand>(p => p
            .Add(x => x.Hand, hand)
            .Add(x => x.Label, "Dealer")
            .Add(x => x.IsDealer, true)
            .Add(x => x.HideSecondCard, true)
            .Add(x => x.ShowScore, true));

        var score = cut.Find(".hand-score");
        Assert.Equal("10", score.TextContent);
    }

    [Fact]
    public void Dealer_HideSecondCard_SecondCardIsFaceDown()
    {
        var hand = CreateHand(
            new Card(Suit.Hearts, Rank.Ten),
            new Card(Suit.Clubs, Rank.Seven));

        var cut = Render<CardHand>(p => p
            .Add(x => x.Hand, hand)
            .Add(x => x.Label, "Dealer")
            .Add(x => x.IsDealer, true)
            .Add(x => x.HideSecondCard, true));

        var cards = cut.FindAll(".playing-card");
        Assert.Equal(2, cards.Count);
        // Second card should be face-down
        Assert.NotNull(cards[1].QuerySelector(".card-back"));
    }

    [Fact]
    public void BustHand_ShowsBustInScore()
    {
        var hand = CreateHand(
            new Card(Suit.Hearts, Rank.Ten),
            new Card(Suit.Clubs, Rank.Eight),
            new Card(Suit.Spades, Rank.Five));

        var cut = Render<CardHand>(p => p
            .Add(x => x.Hand, hand)
            .Add(x => x.Label, "Player")
            .Add(x => x.ShowScore, true));

        var score = cut.Find(".hand-score");
        Assert.Contains("BUST", score.TextContent);
        Assert.Contains("bust", score.ClassList);
    }

    [Fact]
    public void BlackjackHand_ShowsBlackjackInScore()
    {
        var hand = CreateHand(
            new Card(Suit.Hearts, Rank.Ace),
            new Card(Suit.Spades, Rank.King));

        var cut = Render<CardHand>(p => p
            .Add(x => x.Hand, hand)
            .Add(x => x.Label, "Player")
            .Add(x => x.ShowScore, true));

        var score = cut.Find(".hand-score");
        Assert.Equal("BLACKJACK!", score.TextContent);
        Assert.Contains("blackjack", score.ClassList);
    }

    [Fact]
    public void RendersCorrectNumberOfCards()
    {
        var hand = CreateHand(
            new Card(Suit.Hearts, Rank.Five),
            new Card(Suit.Clubs, Rank.Eight),
            new Card(Suit.Spades, Rank.Two));

        var cut = Render<CardHand>(p => p
            .Add(x => x.Hand, hand)
            .Add(x => x.Label, "Player"));

        var cards = cut.FindAll(".playing-card");
        Assert.Equal(3, cards.Count);
    }
}
