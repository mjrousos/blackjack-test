using Xunit;

namespace Blackjack.Domain.Tests.Models;

using Blackjack.Domain.Models;
using FluentAssertions;

public class GameSettingsTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        var settings = new GameSettings();
        settings.DeckCount.Should().Be(6);
        settings.MinBet.Should().Be(5m);
        settings.MaxBet.Should().Be(500m);
        settings.StartingBalance.Should().Be(1000m);
        settings.ReshufflePenetration.Should().Be(0.75);
        settings.DealerHitsOnSoft17.Should().BeFalse();
    }

    [Fact]
    public void CustomValues_CanBeSet()
    {
        var settings = new GameSettings
        {
            DeckCount = 4,
            MinBet = 10m,
            MaxBet = 200m,
            StartingBalance = 500m,
            ReshufflePenetration = 0.5,
            DealerHitsOnSoft17 = true
        };

        settings.DeckCount.Should().Be(4);
        settings.MinBet.Should().Be(10m);
        settings.MaxBet.Should().Be(200m);
        settings.StartingBalance.Should().Be(500m);
        settings.ReshufflePenetration.Should().Be(0.5);
        settings.DealerHitsOnSoft17.Should().BeTrue();
    }

    [Fact]
    public void InvalidDeckCount_ThrowsWhenUsedInShoe()
    {
        // GameSettings init properties bypass constructor validation,
        // but invalid DeckCount is caught by the Shoe constructor
        var settings = new GameSettings { DeckCount = 0 };
        var act = () => new Shoe(settings.DeckCount);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void DeckCount_TooHigh_ThrowsWhenUsedInShoe()
    {
        var settings = new GameSettings { DeckCount = 10 };
        var act = () => new Shoe(settings.DeckCount);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
