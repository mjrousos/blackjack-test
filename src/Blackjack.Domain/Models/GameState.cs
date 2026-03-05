namespace Blackjack.Domain.Models;

public enum GameState
{
    WaitingForBet,
    Dealing,
    PlayerTurn,
    DealerTurn,
    Resolved
}
