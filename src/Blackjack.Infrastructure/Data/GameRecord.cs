namespace Blackjack.Infrastructure.Data;

using Blackjack.Domain.Models;

public class GameRecord
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime EndedAt { get; set; }
    public decimal InitialBet { get; set; }
    public decimal FinalPayout { get; set; }
    public GameResult Result { get; set; }
    public string PlayerHandSummary { get; set; } = string.Empty;  // e.g., "A♠ K♥"
    public string DealerHandSummary { get; set; } = string.Empty;
    public int PlayerScore { get; set; }
    public int DealerScore { get; set; }
}
