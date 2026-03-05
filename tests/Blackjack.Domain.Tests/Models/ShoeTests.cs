using Xunit;

namespace Blackjack.Domain.Tests.Models;

using Blackjack.Domain.Models;
using FluentAssertions;

public class ShoeTests
{
    [Theory]
    [InlineData(1, 52)]
    [InlineData(2, 104)]
    [InlineData(6, 312)]
    public void Constructor_CreatesCorrectNumberOfCards(int deckCount, int expectedCards)
    {
        var shoe = new Shoe(deckCount);
        shoe.CardsRemaining.Should().Be(expectedCards);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(9)]
    public void Constructor_InvalidDeckCount_Throws(int deckCount)
    {
        var act = () => new Shoe(deckCount);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Draw_ReturnsCardAndDecrementsCount()
    {
        var shoe = new Shoe(1);
        int initialCount = shoe.CardsRemaining;
        var card = shoe.Draw();
        card.Should().NotBeNull();
        shoe.CardsRemaining.Should().Be(initialCount - 1);
    }

    [Fact]
    public void Draw_FromEmptyShoe_ThrowsInvalidOperationException()
    {
        var shoe = new Shoe(1);
        for (int i = 0; i < 52; i++)
            shoe.Draw();

        var act = () => shoe.Draw();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void PenetrationReached_TriggersAt75Percent()
    {
        var shoe = new Shoe(1); // 52 cards, 75% = 39 dealt
        for (int i = 0; i < 38; i++)
            shoe.Draw();
        shoe.PenetrationReached.Should().BeFalse();

        shoe.Draw(); // 39th card dealt
        shoe.PenetrationReached.Should().BeTrue();
    }

    [Fact]
    public void Shuffle_ResetsCardCount()
    {
        var shoe = new Shoe(1);
        shoe.Draw();
        shoe.Draw();
        shoe.CardsRemaining.Should().Be(50);

        shoe.Shuffle();
        shoe.CardsRemaining.Should().Be(52);
    }

    [Fact]
    public void Cards_AreShuffled_WithDifferentSeeds()
    {
        var shoe1 = new Shoe(1, new Random(42));
        var shoe2 = new Shoe(1, new Random(99));

        var cards1 = Enumerable.Range(0, 10).Select(_ => shoe1.Draw()).ToList();
        var cards2 = Enumerable.Range(0, 10).Select(_ => shoe2.Draw()).ToList();

        // Different seeds should produce different card orders
        cards1.Should().NotEqual(cards2);
    }
}
