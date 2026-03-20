using Blackjack.Domain.Models;

namespace Blackjack.Domain.Services;

public class BlackjackGame : IGameService
{
    public GameSettings Settings { get; }
    public GameState State { get; private set; } = GameState.WaitingForBet;
    public Hand PlayerHand { get; } = new();
    public List<Hand> SplitHands { get; } = [];
    public int ActiveHandIndex { get; private set; }
    public Hand DealerHand { get; } = new();
    public decimal CurrentBet { get; private set; }
    public List<decimal> SplitBets { get; } = [];
    public decimal PlayerBalance { get; private set; }
    public GameResult? Result { get; private set; }
    public List<GameResult?> SplitResults { get; } = [];
    public bool IsDealerCardHidden { get; private set; }
    public Shoe Shoe { get; }

    public BlackjackGame(GameSettings settings, Random? random = null)
    {
        Settings = settings;
        PlayerBalance = settings.StartingBalance;
        Shoe = new Shoe(settings.DeckCount, random);
    }

    public void PlaceBet(decimal amount)
    {
        if (State != GameState.WaitingForBet)
            throw new InvalidOperationException($"Cannot place bet in {State} state.");

        if (amount < Settings.MinBet)
            throw new ArgumentException($"Bet must be at least {Settings.MinBet}.", nameof(amount));
        if (amount > Settings.MaxBet)
            throw new ArgumentException($"Bet must be at most {Settings.MaxBet}.", nameof(amount));
        if (amount > PlayerBalance)
            throw new ArgumentException("Insufficient balance.", nameof(amount));

        CurrentBet = amount;
        PlayerBalance -= amount;
        SplitBets.Clear();
        SplitBets.Add(amount);
        State = GameState.Dealing;
    }

    public void Deal()
    {
        if (State != GameState.Dealing)
            throw new InvalidOperationException($"Cannot deal in {State} state.");

        if (Shoe.PenetrationReached)
            Shoe.Shuffle();

        // Deal alternating: player, dealer, player, dealer
        PlayerHand.AddCard(Shoe.Draw());
        DealerHand.AddCard(Shoe.Draw());
        PlayerHand.AddCard(Shoe.Draw());
        DealerHand.AddCard(Shoe.Draw());

        IsDealerCardHidden = true;

        // Check for natural blackjacks
        bool playerBj = PlayerHand.IsBlackjack;
        bool dealerBj = DealerHand.IsBlackjack;

        if (playerBj && dealerBj)
        {
            IsDealerCardHidden = false;
            Result = GameResult.Push;
            SplitResults.Clear();
            SplitResults.Add(GameResult.Push);
            PlayerBalance += CalculatePayout();
            State = GameState.Resolved;
        }
        else if (playerBj)
        {
            IsDealerCardHidden = false;
            Result = GameResult.Blackjack;
            SplitResults.Clear();
            SplitResults.Add(GameResult.Blackjack);
            PlayerBalance += CalculatePayout();
            State = GameState.Resolved;
        }
        else if (dealerBj)
        {
            IsDealerCardHidden = false;
            Result = GameResult.Lose;
            SplitResults.Clear();
            SplitResults.Add(GameResult.Lose);
            PlayerBalance += CalculatePayout();
            State = GameState.Resolved;
        }
        else
        {
            State = GameState.PlayerTurn;
            ActiveHandIndex = 0;
        }
    }

    public void Hit()
    {
        if (State != GameState.PlayerTurn)
            throw new InvalidOperationException($"Cannot hit in {State} state.");

        var activeHand = GetActiveHand();
        activeHand.AddCard(Shoe.Draw());

        if (activeHand.IsBust)
        {
            AdvanceAfterHandComplete();
        }
    }

    public void Stand()
    {
        if (State != GameState.PlayerTurn)
            throw new InvalidOperationException($"Cannot stand in {State} state.");

        AdvanceAfterHandComplete();
    }

    public void DoubleDown()
    {
        if (State != GameState.PlayerTurn)
            throw new InvalidOperationException($"Cannot double down in {State} state.");

        var activeHand = GetActiveHand();

        if (activeHand.Cards.Count != 2)
            throw new InvalidOperationException("Can only double down on a hand with exactly 2 cards.");

        decimal additionalBet = SplitBets[ActiveHandIndex];
        if (additionalBet > PlayerBalance)
            throw new InvalidOperationException("Insufficient balance to double down.");

        PlayerBalance -= additionalBet;
        SplitBets[ActiveHandIndex] *= 2;

        activeHand.AddCard(Shoe.Draw());

        AdvanceAfterHandComplete();
    }

    public void Split()
    {
        if (State != GameState.PlayerTurn)
            throw new InvalidOperationException($"Cannot split in {State} state.");

        if (SplitHands.Count > 0)
            throw new InvalidOperationException("Only one split is allowed.");

        var activeHand = GetActiveHand();

        if (!activeHand.CanSplit)
            throw new InvalidOperationException("Hand cannot be split.");

        decimal additionalBet = SplitBets[0];
        if (additionalBet > PlayerBalance)
            throw new InvalidOperationException("Insufficient balance to split.");

        PlayerBalance -= additionalBet;

        // Take the second card from the player hand to create the split hand
        var secondCard = PlayerHand.Cards[1];
        var firstCard = PlayerHand.Cards[0];
        bool splittingAces = firstCard.Rank == Rank.Ace;

        PlayerHand.Clear();
        PlayerHand.AddCard(firstCard);

        var splitHand = new Hand();
        splitHand.AddCard(secondCard);
        SplitHands.Add(splitHand);

        SplitBets.Add(additionalBet);
        SplitResults.Clear();
        SplitResults.Add(null);
        SplitResults.Add(null);

        // Deal one card to each hand
        PlayerHand.AddCard(Shoe.Draw());
        splitHand.AddCard(Shoe.Draw());

        if (splittingAces)
        {
            // Splitting aces: no further action, auto-stand both
            State = GameState.DealerTurn;
            // Check if both hands bust (unlikely with aces but be safe)
            if (PlayerHand.IsBust && splitHand.IsBust)
            {
                ResolveAllHands();
                State = GameState.Resolved;
            }
        }
        else
        {
            ActiveHandIndex = 0;
        }
    }

    public void Surrender()
    {
        if (State != GameState.PlayerTurn)
            throw new InvalidOperationException($"Cannot surrender in {State} state.");

        var activeHand = GetActiveHand();

        if (activeHand.Cards.Count != 2)
            throw new InvalidOperationException("Can only surrender on the initial two-card hand.");

        if (SplitHands.Count > 0)
            throw new InvalidOperationException("Cannot surrender after splitting.");

        IsDealerCardHidden = false;
        Result = GameResult.Surrender;
        SplitResults.Clear();
        SplitResults.Add(GameResult.Surrender);
        PlayerBalance += CalculatePayout();
        State = GameState.Resolved;
    }

    public void ResolveDealerTurn()
    {
        if (State != GameState.DealerTurn)
            throw new InvalidOperationException($"Cannot resolve dealer turn in {State} state.");

        IsDealerCardHidden = false;

        // Dealer draws per rules
        while (ShouldDealerHit())
        {
            DealerHand.AddCard(Shoe.Draw());
        }

        ResolveAllHands();
        State = GameState.Resolved;
        PlayerBalance += CalculatePayout();
    }

    public List<GameAction> GetAvailableActions()
    {
        if (State != GameState.PlayerTurn)
            return [];

        var actions = new List<GameAction> { GameAction.Hit, GameAction.Stand };
        var activeHand = GetActiveHand();

        if (activeHand.Cards.Count == 2 && SplitBets[ActiveHandIndex] <= PlayerBalance)
            actions.Add(GameAction.DoubleDown);

        if (activeHand.CanSplit && SplitHands.Count == 0 && SplitBets[0] <= PlayerBalance)
            actions.Add(GameAction.Split);

        if (activeHand.Cards.Count == 2 && SplitHands.Count == 0)
            actions.Add(GameAction.Surrender);

        return actions;
    }

    public decimal CalculatePayout()
    {
        decimal total = 0m;

        if (SplitHands.Count == 0)
        {
            // No split - use Result and CurrentBet
            total = CalculateHandPayout(Result, SplitBets.Count > 0 ? SplitBets[0] : CurrentBet);
        }
        else
        {
            // Split - calculate per hand
            for (int i = 0; i < SplitResults.Count; i++)
            {
                total += CalculateHandPayout(SplitResults[i], SplitBets[i]);
            }
        }

        return total;
    }

    public void StartNewRound()
    {
        if (State != GameState.Resolved)
            throw new InvalidOperationException($"Cannot start new round in {State} state.");

        PlayerHand.Clear();
        DealerHand.Clear();
        SplitHands.Clear();
        SplitBets.Clear();
        SplitResults.Clear();
        CurrentBet = 0;
        Result = null;
        ActiveHandIndex = 0;
        IsDealerCardHidden = false;
        State = GameState.WaitingForBet;

        if (Shoe.PenetrationReached)
            Shoe.Shuffle();
    }

    public void RefillBalance()
    {
        if (PlayerBalance == 0)
            PlayerBalance = Settings.StartingBalance;
    }

    public void SetBalance(decimal balance)
    {
        PlayerBalance = balance;
    }

    private Hand GetActiveHand()
    {
        if (ActiveHandIndex == 0)
            return PlayerHand;

        return SplitHands[ActiveHandIndex - 1];
    }

    private void AdvanceAfterHandComplete()
    {
        if (SplitHands.Count > 0 && ActiveHandIndex < SplitHands.Count)
        {
            // More split hands to play
            ActiveHandIndex++;
        }
        else
        {
            // All hands played - check if all busted
            bool allBust = PlayerHand.IsBust &&
                           SplitHands.All(h => h.IsBust);

            if (allBust)
            {
                IsDealerCardHidden = false;
                ResolveAllHands();
                State = GameState.Resolved;
                PlayerBalance += CalculatePayout();
            }
            else
            {
                State = GameState.DealerTurn;
            }
        }
    }

    private bool ShouldDealerHit()
    {
        int score = DealerHand.Score;

        if (score < 17)
            return true;

        if (score == 17 && DealerHand.IsSoft && Settings.DealerHitsOnSoft17)
            return true;

        return false;
    }

    private void ResolveAllHands()
    {
        if (SplitHands.Count == 0)
        {
            // No split
            Result = DetermineResult(PlayerHand);
            SplitResults.Clear();
            SplitResults.Add(Result);
        }
        else
        {
            // Split - resolve each hand
            SplitResults.Clear();
            SplitResults.Add(DetermineResult(PlayerHand));
            for (int i = 0; i < SplitHands.Count; i++)
            {
                SplitResults.Add(DetermineResult(SplitHands[i]));
            }
            Result = SplitResults[0];
        }
    }

    private GameResult DetermineResult(Hand playerHand)
    {
        if (playerHand.IsBust)
            return GameResult.Lose;

        if (DealerHand.IsBust)
            return GameResult.Win;

        if (playerHand.Score > DealerHand.Score)
            return GameResult.Win;

        if (playerHand.Score < DealerHand.Score)
            return GameResult.Lose;

        return GameResult.Push;
    }

    private static decimal CalculateHandPayout(GameResult? result, decimal bet) => result switch
    {
        GameResult.Win => bet * 2,
        GameResult.Blackjack => bet * 2.5m,
        GameResult.Push => bet,
        GameResult.Surrender => bet / 2,
        GameResult.Lose => 0m,
        _ => 0m
    };
}
