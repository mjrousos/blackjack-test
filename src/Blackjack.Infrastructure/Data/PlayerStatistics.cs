namespace Blackjack.Infrastructure.Data;

public record PlayerStatistics
{
    public int TotalGames { get; init; }
    public int Wins { get; init; }
    public int Losses { get; init; }
    public int Pushes { get; init; }
    public int Blackjacks { get; init; }
    public decimal BiggestWin { get; init; }
    public decimal TotalWagered { get; init; }
    public decimal NetProfitLoss { get; init; }
    public double WinRate => TotalGames > 0 ? (double)(Wins + Blackjacks) / TotalGames * 100 : 0;
}
