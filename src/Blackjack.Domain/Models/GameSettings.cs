namespace Blackjack.Domain.Models;

public record GameSettings
{
    public int DeckCount { get; init; } = 6;
    public decimal MinBet { get; init; } = 5m;
    public decimal MaxBet { get; init; } = 500m;
    public decimal StartingBalance { get; init; } = 1000m;
    public double ReshufflePenetration { get; init; } = 0.75;
    public bool DealerHitsOnSoft17 { get; init; } = false;

    public GameSettings()
    {
        Validate();
    }

    private void Validate()
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(DeckCount, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(DeckCount, 8);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(MinBet, 0m);
        ArgumentOutOfRangeException.ThrowIfLessThan(MaxBet, MinBet);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(StartingBalance, 0m);
        ArgumentOutOfRangeException.ThrowIfLessThan(ReshufflePenetration, 0.0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(ReshufflePenetration, 1.0);
    }
}
