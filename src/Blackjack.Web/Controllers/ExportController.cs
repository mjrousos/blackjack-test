using Blackjack.Domain.Models;
using Blackjack.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;

namespace Blackjack.Web.Controllers;

[Authorize]
[Route("api/[controller]")]
public class ExportController(IGameHistoryRepository gameHistoryRepository) : Controller
{
    [HttpGet("history/csv")]
    public async Task<IActionResult> ExportHistoryCsv()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var records = await gameHistoryRepository.GetAllHistoryAsync(userId);

        var sb = new StringBuilder();
        sb.AppendLine("Date,Bet,Player Hand,Dealer Hand,Result,Payout,Net");

        foreach (var game in records)
        {
            var net = game.FinalPayout - game.InitialBet;
            sb.AppendLine(string.Join(",",
                EscapeCsvField(game.EndedAt.ToString("yyyy-MM-dd HH:mm:ss") + " UTC"),
                EscapeCsvField(game.InitialBet.ToString("N2")),
                EscapeCsvField(game.PlayerHandSummary),
                EscapeCsvField(game.DealerHandSummary),
                EscapeCsvField(GetResultDisplay(game.Result)),
                EscapeCsvField(game.FinalPayout.ToString("N2")),
                EscapeCsvField(net.ToString("N2"))
            ));
        }

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(bytes, "text/csv", "game-history.csv");
    }

    private static string EscapeCsvField(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }
        return value;
    }

    private static string GetResultDisplay(GameResult result) => result switch
    {
        GameResult.Win => "Win",
        GameResult.Blackjack => "Blackjack",
        GameResult.Push => "Push",
        GameResult.Lose => "Loss",
        _ => result.ToString()
    };
}
