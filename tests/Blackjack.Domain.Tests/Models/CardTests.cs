using Xunit;

namespace Blackjack.Domain.Tests.Models;

using Blackjack.Domain.Models;
using FluentAssertions;

public class CardTests
{
    [Fact]
    public void AceValue_ShouldBe11()
    {
        var card = new Card(Suit.Hearts, Rank.Ace);
        card.Value.Should().Be(11);
    }

    [Theory]
    [InlineData(Rank.Jack)]
    [InlineData(Rank.Queen)]
    [InlineData(Rank.King)]
    public void FaceCardValue_ShouldBe10(Rank rank)
    {
        var card = new Card(Suit.Hearts, rank);
        card.Value.Should().Be(10);
    }

    [Theory]
    [InlineData(Rank.Two, 2)]
    [InlineData(Rank.Three, 3)]
    [InlineData(Rank.Four, 4)]
    [InlineData(Rank.Five, 5)]
    [InlineData(Rank.Six, 6)]
    [InlineData(Rank.Seven, 7)]
    [InlineData(Rank.Eight, 8)]
    [InlineData(Rank.Nine, 9)]
    [InlineData(Rank.Ten, 10)]
    public void NumberCardValue_ShouldBeNumericValue(Rank rank, int expectedValue)
    {
        var card = new Card(Suit.Hearts, rank);
        card.Value.Should().Be(expectedValue);
    }

    [Fact]
    public void DisplayName_ShouldBeFormatted()
    {
        var card = new Card(Suit.Spades, Rank.Ace);
        card.DisplayName.Should().Be("Ace of Spades");
    }

    [Fact]
    public void ToString_ShouldReturnDisplayName()
    {
        var card = new Card(Suit.Diamonds, Rank.King);
        card.ToString().Should().Be(card.DisplayName);
    }
}
