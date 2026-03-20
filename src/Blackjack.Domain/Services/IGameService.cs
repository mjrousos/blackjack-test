using Blackjack.Domain.Models;

namespace Blackjack.Domain.Services;

public interface IGameService
{
    GameSettings Settings { get; }
    GameState State { get; }
    Hand PlayerHand { get; }
    List<Hand> SplitHands { get; }
    int ActiveHandIndex { get; }
    Hand DealerHand { get; }
    decimal CurrentBet { get; }
    List<decimal> SplitBets { get; }
    decimal PlayerBalance { get; }
    GameResult? Result { get; }
    List<GameResult?> SplitResults { get; }
    bool IsDealerCardHidden { get; }
    decimal InsuranceBet { get; }

    void PlaceBet(decimal amount);
    void Deal();
    void Hit();
    void Stand();
    void DoubleDown();
    void Split();
    void TakeInsurance();
    void DeclineInsurance();
    void ResolveDealerTurn();
    List<GameAction> GetAvailableActions();
    decimal CalculatePayout();
    void StartNewRound();
    void RefillBalance();
    void SetBalance(decimal balance);
}
