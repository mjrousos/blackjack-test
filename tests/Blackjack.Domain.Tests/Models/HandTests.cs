using Xunit;

namespace Blackjack.Domain.Tests.Models;

using Blackjack.Domain.Models;
using FluentAssertions;

public class HandTests
{
    [Fact]
    public void EmptyHand_ShouldHaveScoreZero()
    {
        var hand = new Hand();
        hand.Score.Should().Be(0);
    }

    [Fact]
    public void SimpleHand_ShouldSumCorrectly()
    {
        var hand = new Hand();
        hand.AddCard(new Card(Suit.Hearts, Rank.Five));
        hand.AddCard(new Card(Suit.Clubs, Rank.Seven));
        hand.Score.Should().Be(12);
    }

    [Fact]
    public void SingleAce_ShouldCountAs11()
    {
        var hand = new Hand();
        hand.AddCard(new Card(Suit.Hearts, Rank.Ace));
        hand.Score.Should().Be(11);
    }

    [Fact]
    public void Ace_ShouldAdjustFrom11To1_WhenTotalExceeds21()
    {
        var hand = new Hand();
        hand.AddCard(new Card(Suit.Hearts, Rank.Ace));
        hand.AddCard(new Card(Suit.Hearts, Rank.Eight));
        hand.AddCard(new Card(Suit.Clubs, Rank.Six));
        // 11 + 8 + 6 = 25 → ace adjusts: 1 + 8 + 6 = 15
        hand.Score.Should().Be(15);
    }

    [Fact]
    public void TwoAces_ShouldBe12()
    {
        var hand = new Hand();
        hand.AddCard(new Card(Suit.Hearts, Rank.Ace));
        hand.AddCard(new Card(Suit.Clubs, Rank.Ace));
        // 11 + 11 = 22 → one ace adjusts: 1 + 11 = 12
        hand.Score.Should().Be(12);
    }

    [Fact]
    public void TwoAcesAndNine_ShouldBe21()
    {
        var hand = new Hand();
        hand.AddCard(new Card(Suit.Hearts, Rank.Ace));
        hand.AddCard(new Card(Suit.Clubs, Rank.Ace));
        hand.AddCard(new Card(Suit.Diamonds, Rank.Nine));
        // 11 + 11 + 9 = 31 → adjust one: 1 + 11 + 9 = 21
        hand.Score.Should().Be(21);
    }

    [Fact]
    public void IsSoft_WhenAceCountedAsEleven()
    {
        // IsSoft is true when the hand has an ace that is counted as 11
        var hand = new Hand();
        hand.AddCard(new Card(Suit.Hearts, Rank.Ace));
        hand.AddCard(new Card(Suit.Hearts, Rank.Six));
        // Raw sum: 11+6=17, Score: 17, ace is counted as 11 → soft
        hand.IsSoft.Should().BeTrue();
    }

    [Fact]
    public void IsNotSoft_WhenAceAdjustedToOne()
    {
        // When ace is forced to 1 (adjusted down), IsSoft is false
        var hand = new Hand();
        hand.AddCard(new Card(Suit.Hearts, Rank.Ace));
        hand.AddCard(new Card(Suit.Hearts, Rank.Eight));
        hand.AddCard(new Card(Suit.Clubs, Rank.Six));
        // Raw sum: 11+8+6=25, Score: 1+8+6=15, ace was adjusted → not soft
        hand.IsSoft.Should().BeFalse();
    }

    [Fact]
    public void IsBust_WhenScoreExceeds21()
    {
        var hand = new Hand();
        hand.AddCard(new Card(Suit.Hearts, Rank.Ten));
        hand.AddCard(new Card(Suit.Clubs, Rank.Eight));
        hand.AddCard(new Card(Suit.Diamonds, Rank.Five));
        // 10 + 8 + 5 = 23
        hand.IsBust.Should().BeTrue();
    }

    [Fact]
    public void IsBlackjack_WithAceAndKing()
    {
        var hand = new Hand();
        hand.AddCard(new Card(Suit.Hearts, Rank.Ace));
        hand.AddCard(new Card(Suit.Spades, Rank.King));
        hand.IsBlackjack.Should().BeTrue();
    }

    [Fact]
    public void IsBlackjack_FalseWith3CardsTotaling21()
    {
        var hand = new Hand();
        hand.AddCard(new Card(Suit.Hearts, Rank.Seven));
        hand.AddCard(new Card(Suit.Clubs, Rank.Seven));
        hand.AddCard(new Card(Suit.Diamonds, Rank.Seven));
        hand.Score.Should().Be(21);
        hand.IsBlackjack.Should().BeFalse();
    }

    [Fact]
    public void CanSplit_WithMatchingRanks()
    {
        var hand = new Hand();
        hand.AddCard(new Card(Suit.Hearts, Rank.Eight));
        hand.AddCard(new Card(Suit.Clubs, Rank.Eight));
        hand.CanSplit.Should().BeTrue();
    }

    [Fact]
    public void CanSplit_FalseWithDifferentRanks()
    {
        var hand = new Hand();
        hand.AddCard(new Card(Suit.Hearts, Rank.Eight));
        hand.AddCard(new Card(Suit.Clubs, Rank.Seven));
        hand.CanSplit.Should().BeFalse();
    }

    [Fact]
    public void CanSplit_FalseWithMoreThan2Cards()
    {
        var hand = new Hand();
        hand.AddCard(new Card(Suit.Hearts, Rank.Eight));
        hand.AddCard(new Card(Suit.Clubs, Rank.Eight));
        hand.AddCard(new Card(Suit.Diamonds, Rank.Eight));
        hand.CanSplit.Should().BeFalse();
    }

    [Fact]
    public void Clear_RemovesAllCards()
    {
        var hand = new Hand();
        hand.AddCard(new Card(Suit.Hearts, Rank.Ace));
        hand.AddCard(new Card(Suit.Clubs, Rank.King));
        hand.Clear();
        hand.Cards.Should().BeEmpty();
        hand.Score.Should().Be(0);
    }

    [Fact]
    public void IsNotSoft_WhenNoAces()
    {
        var hand = new Hand();
        hand.AddCard(new Card(Suit.Hearts, Rank.Ten));
        hand.AddCard(new Card(Suit.Clubs, Rank.Seven));
        hand.IsSoft.Should().BeFalse();
    }

    [Fact]
    public void IsNotSoft_WhenBust()
    {
        var hand = new Hand();
        hand.AddCard(new Card(Suit.Hearts, Rank.Ten));
        hand.AddCard(new Card(Suit.Clubs, Rank.Eight));
        hand.AddCard(new Card(Suit.Diamonds, Rank.Ace));
        hand.AddCard(new Card(Suit.Spades, Rank.Five));
        // 10+8+1+5=24, bust — IsSoft should be false even with an ace
        hand.IsBust.Should().BeTrue();
        hand.IsSoft.Should().BeFalse();
    }

    [Fact]
    public void IsSoft_WithTwoAces_OneCountedAsEleven()
    {
        var hand = new Hand();
        hand.AddCard(new Card(Suit.Hearts, Rank.Ace));
        hand.AddCard(new Card(Suit.Clubs, Rank.Ace));
        // Score: 12 (one ace as 11, one as 1) → soft
        hand.IsSoft.Should().BeTrue();
    }

    [Fact]
    public void IsNotBust_WhenScoreAt21()
    {
        var hand = new Hand();
        hand.AddCard(new Card(Suit.Hearts, Rank.Ten));
        hand.AddCard(new Card(Suit.Clubs, Rank.Five));
        hand.AddCard(new Card(Suit.Diamonds, Rank.Six));
        hand.Score.Should().Be(21);
        hand.IsBust.Should().BeFalse();
    }

    [Fact]
    public void IsBlackjack_WithAceAndTen()
    {
        var hand = new Hand();
        hand.AddCard(new Card(Suit.Diamonds, Rank.Ace));
        hand.AddCard(new Card(Suit.Clubs, Rank.Ten));
        hand.IsBlackjack.Should().BeTrue();
    }

    [Fact]
    public void ToString_FormatsCorrectly()
    {
        var hand = new Hand();
        hand.AddCard(new Card(Suit.Spades, Rank.Ace));
        hand.AddCard(new Card(Suit.Hearts, Rank.King));
        hand.ToString().Should().Be("A♠ K♥ (21)");
    }

    [Fact]
    public void ToString_ShowsTenCorrectly()
    {
        var hand = new Hand();
        hand.AddCard(new Card(Suit.Diamonds, Rank.Ten));
        hand.AddCard(new Card(Suit.Clubs, Rank.Five));
        hand.ToString().Should().Be("10♦ 5♣ (15)");
    }

    [Theory]
    [InlineData(Rank.Jack, "J")]
    [InlineData(Rank.Queen, "Q")]
    [InlineData(Rank.King, "K")]
    [InlineData(Rank.Ace, "A")]
    [InlineData(Rank.Ten, "10")]
    [InlineData(Rank.Two, "2")]
    [InlineData(Rank.Nine, "9")]
    public void ToString_ShowsRankAbbreviations(Rank rank, string expected)
    {
        var hand = new Hand();
        hand.AddCard(new Card(Suit.Spades, rank));
        hand.ToString().Should().StartWith($"{expected}♠");
    }

    [Theory]
    [InlineData(Suit.Hearts, '♥')]
    [InlineData(Suit.Diamonds, '♦')]
    [InlineData(Suit.Clubs, '♣')]
    [InlineData(Suit.Spades, '♠')]
    public void ToString_ShowsSuitSymbols(Suit suit, char expected)
    {
        var hand = new Hand();
        hand.AddCard(new Card(suit, Rank.Five));
        hand.ToString().Should().Contain(expected.ToString());
    }

    [Fact]
    public void ToString_EmptyHand()
    {
        var hand = new Hand();
        hand.ToString().Should().Be(" (0)");
    }
}
