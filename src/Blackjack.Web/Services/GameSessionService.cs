namespace Blackjack.Web.Services;

using Blackjack.Domain.Models;
using Blackjack.Domain.Services;
using Blackjack.Infrastructure.Data;
using Blackjack.Infrastructure.Repositories;
using System.Security.Claims;

public class GameSessionService
{
    private readonly IPlayerRepository _playerRepository;
    private readonly IGameHistoryRepository _gameHistoryRepository;
    private BlackjackGame _game;
    private string? _userId;
    private DateTime _roundStartTime;

    public GameSessionService(IPlayerRepository playerRepository, IGameHistoryRepository gameHistoryRepository)
    {
        _playerRepository = playerRepository;
        _gameHistoryRepository = gameHistoryRepository;
        _game = new BlackjackGame(new GameSettings());
    }

    public GameState State => _game.State;
    public Hand PlayerHand => _game.PlayerHand;
    public List<Hand> SplitHands => _game.SplitHands;
    public int ActiveHandIndex => _game.ActiveHandIndex;
    public Hand DealerHand => _game.DealerHand;
    public decimal CurrentBet => _game.CurrentBet;
    public List<decimal> SplitBets => _game.SplitBets;
    public decimal PlayerBalance => _game.PlayerBalance;
    public GameResult? Result => _game.Result;
    public List<GameResult?> SplitResults => _game.SplitResults;
    public bool IsDealerCardHidden => _game.IsDealerCardHidden;
    public decimal InsuranceBet => _game.InsuranceBet;
    public List<GameAction> AvailableActions => _game.GetAvailableActions();
    public decimal LastPayout { get; private set; }

    public async Task InitializeAsync(ClaimsPrincipal user)
    {
        _userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (_userId != null)
        {
            var balance = await _playerRepository.GetBalanceAsync(_userId);
            _game.SetBalance(balance);
        }
    }

    public void PlaceBet(decimal amount)
    {
        _game.PlaceBet(amount);
        _roundStartTime = DateTime.UtcNow;
    }

    public void Deal() => _game.Deal();

    public void Hit() => _game.Hit();

    public void Stand()
    {
        _game.Stand();
        if (_game.State == GameState.DealerTurn)
        {
            _game.ResolveDealerTurn();
        }
    }

    public void DoubleDown()
    {
        _game.DoubleDown();
        if (_game.State == GameState.DealerTurn)
        {
            _game.ResolveDealerTurn();
        }
    }

    public void Split()
    {
        _game.Split();
        if (_game.State == GameState.DealerTurn)
        {
            _game.ResolveDealerTurn();
        }
    }

    public void TakeInsurance() => _game.TakeInsurance();

    public void DeclineInsurance() => _game.DeclineInsurance();

    public void CheckAndResolveDealerTurn()
    {
        if (_game.State == GameState.DealerTurn)
        {
            _game.ResolveDealerTurn();
        }
    }

    public async Task FinalizeRoundAsync()
    {
        if (_userId == null || _game.State != GameState.Resolved) return;

        LastPayout = _game.CalculatePayout();

        await _playerRepository.UpdateBalanceAsync(_userId, _game.PlayerBalance);

        var record = new GameRecord
        {
            UserId = _userId,
            StartedAt = _roundStartTime,
            EndedAt = DateTime.UtcNow,
            InitialBet = _game.CurrentBet,
            FinalPayout = LastPayout,
            Result = _game.Result ?? GameResult.Lose,
            PlayerHandSummary = _game.PlayerHand.ToString(),
            DealerHandSummary = _game.DealerHand.ToString(),
            PlayerScore = _game.PlayerHand.Score,
            DealerScore = _game.DealerHand.Score
        };
        await _gameHistoryRepository.SaveGameRecordAsync(record);
    }

    public void StartNewRound()
    {
        _game.StartNewRound();
        LastPayout = 0;
    }

    public async Task RefillBalanceAsync()
    {
        _game.RefillBalance();
        if (_userId != null)
        {
            await _playerRepository.RefillBalanceAsync(_userId, _game.Settings.StartingBalance);
        }
    }
}
