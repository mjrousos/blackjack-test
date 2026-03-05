namespace Blackjack.Domain.Models;

public record Card(Suit Suit, Rank Rank)
{
    /// <summary>
    /// Blackjack point value: Ace=11, Face cards=10, others=numeric.
    /// Ace adjustment (11→1) is handled at the Hand level.
    /// </summary>
    public int Value => Rank switch
    {
        Rank.Ace => 11,
        Rank.Jack or Rank.Queen or Rank.King => 10,
        _ => (int)Rank
    };

    public string DisplayName => $"{Rank} of {Suit}";

    public override string ToString() => DisplayName;
}
