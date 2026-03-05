namespace Blackjack.Domain.Models;

public class Hand
{
    private static readonly Dictionary<Suit, char> SuitSymbols = new()
    {
        [Suit.Hearts] = '♥',
        [Suit.Diamonds] = '♦',
        [Suit.Clubs] = '♣',
        [Suit.Spades] = '♠'
    };

    private readonly List<Card> _cards = [];

    public IReadOnlyList<Card> Cards => _cards;

    public void AddCard(Card card) => _cards.Add(card);

    public int Score
    {
        get
        {
            int total = _cards.Sum(c => c.Value);
            int aces = _cards.Count(c => c.Rank == Rank.Ace);

            while (total > 21 && aces > 0)
            {
                total -= 10;
                aces--;
            }

            return total;
        }
    }

    public bool IsSoft
    {
        get
        {
            int aceCount = _cards.Count(c => c.Rank == Rank.Ace);
            if (aceCount == 0 || IsBust) return false;
            int rawSum = _cards.Sum(c => c.Value);
            int adjustedAces = (rawSum - Score) / 10;
            return adjustedAces < aceCount;
        }
    }

    public bool IsBust => Score > 21;

    public bool IsBlackjack => _cards.Count == 2 && Score == 21;

    public bool CanSplit => _cards.Count == 2 && _cards[0].Rank == _cards[1].Rank;

    public void Clear() => _cards.Clear();

    public override string ToString()
    {
        var cardStrings = _cards.Select(c => $"{RankAbbreviation(c.Rank)}{SuitSymbols[c.Suit]}");
        return $"{string.Join(' ', cardStrings)} ({Score})";
    }

    private static string RankAbbreviation(Rank rank) => rank switch
    {
        Rank.Ace => "A",
        Rank.Ten => "10",
        Rank.Jack => "J",
        Rank.Queen => "Q",
        Rank.King => "K",
        _ => ((int)rank).ToString()
    };
}
