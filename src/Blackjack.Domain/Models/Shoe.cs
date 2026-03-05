namespace Blackjack.Domain.Models;

public class Shoe
{
    private readonly Random _random;
    private readonly List<Card> _cards = [];
    private int _totalCards;

    public int DeckCount { get; }

    public int CardsRemaining => _cards.Count;

    public bool PenetrationReached => (_totalCards - _cards.Count) >= (int)(_totalCards * 0.75);

    public Shoe(int deckCount, Random? random = null)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(deckCount, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(deckCount, 8);

        DeckCount = deckCount;
        _random = random ?? Random.Shared;
        Shuffle();
    }

    public void Shuffle()
    {
        _cards.Clear();

        foreach (var suit in Enum.GetValues<Suit>())
        {
            foreach (var rank in Enum.GetValues<Rank>())
            {
                for (int d = 0; d < DeckCount; d++)
                {
                    _cards.Add(new Card(suit, rank));
                }
            }
        }

        _totalCards = _cards.Count;

        // Fisher-Yates shuffle
        for (int i = _cards.Count - 1; i > 0; i--)
        {
            int j = _random.Next(i + 1);
            (_cards[i], _cards[j]) = (_cards[j], _cards[i]);
        }
    }

    public Card Draw()
    {
        if (_cards.Count == 0)
        {
            throw new InvalidOperationException("The shoe is empty.");
        }

        var card = _cards[^1];
        _cards.RemoveAt(_cards.Count - 1);
        return card;
    }
}
