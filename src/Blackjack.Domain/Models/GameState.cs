namespace Blackjack.Domain.Models;

public enum GameState
{
    WaitingForBet,
    Dealing,
    InsuranceOffered,
    PlayerTurn,
    DealerTurn,
    Resolved
}
