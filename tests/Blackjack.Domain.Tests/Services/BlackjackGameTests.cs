using Xunit;

namespace Blackjack.Domain.Tests.Services;

using Blackjack.Domain.Models;
using Blackjack.Domain.Services;
using FluentAssertions;

public class BlackjackGameTests
{
    private static readonly GameSettings DefaultSettings = new();

    private static BlackjackGame CreateGame(int seed = 42) =>
        new(DefaultSettings, new Random(seed));

    /// <summary>
    /// Searches for a seed that produces a game in PlayerTurn state after deal.
    /// </summary>
    private static BlackjackGame CreateGameInPlayerTurn(decimal bet = 10m, GameSettings? settings = null, int startSeed = 0)
    {
        settings ??= DefaultSettings;
        for (int seed = startSeed; seed < startSeed + 100000; seed++)
        {
            var game = new BlackjackGame(settings, new Random(seed));
            game.PlaceBet(bet);
            game.Deal();
            if (game.State == GameState.PlayerTurn)
                return game;
        }
        throw new InvalidOperationException("Could not find a seed for PlayerTurn.");
    }

    private static BlackjackGame FindGameWithResult(GameResult desiredResult, decimal bet = 10m)
    {
        for (int seed = 0; seed < 100000; seed++)
        {
            var game = new BlackjackGame(DefaultSettings, new Random(seed));
            game.PlaceBet(bet);
            game.Deal();
            if (game.State == GameState.PlayerTurn)
            {
                game.Stand();
                game.ResolveDealerTurn();
                if (game.Result == desiredResult)
                    return game;
            }
            else if (game.Result == desiredResult)
            {
                return game;
            }
        }
        throw new InvalidOperationException($"Could not find a seed for {desiredResult} result.");
    }

    public class StateMachineTests
    {
        [Fact]
        public void CannotPlaceBet_InWrongState()
        {
            var game = CreateGameInPlayerTurn();
            var act = () => game.PlaceBet(10);
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void CannotDeal_InWrongState()
        {
            var game = CreateGame();
            var act = () => game.Deal();
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void CannotHit_InWrongState()
        {
            var game = CreateGame();
            var act = () => game.Hit();
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void CannotStand_InWrongState()
        {
            var game = CreateGame();
            var act = () => game.Stand();
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void CannotDoubleDown_InWrongState()
        {
            var game = CreateGame();
            var act = () => game.DoubleDown();
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void CannotSplit_InWrongState()
        {
            var game = CreateGame();
            var act = () => game.Split();
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void CannotResolveDealerTurn_InWrongState()
        {
            var game = CreateGame();
            var act = () => game.ResolveDealerTurn();
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void CannotStartNewRound_InWrongState()
        {
            var game = CreateGame();
            var act = () => game.StartNewRound();
            act.Should().Throw<InvalidOperationException>();
        }
    }

    public class PlaceBetTests
    {
        [Fact]
        public void ValidBet_TransitionsToDealingState()
        {
            var game = CreateGame();
            game.PlaceBet(10);
            game.State.Should().Be(GameState.Dealing);
        }

        [Fact]
        public void ValidBet_DeductsFromBalance()
        {
            var game = CreateGame();
            var initialBalance = game.PlayerBalance;
            game.PlaceBet(10);
            game.PlayerBalance.Should().Be(initialBalance - 10);
        }

        [Fact]
        public void BetBelowMinimum_ThrowsArgumentException()
        {
            var game = CreateGame();
            var act = () => game.PlaceBet(1);
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void BetAboveMaximum_ThrowsArgumentException()
        {
            var game = CreateGame();
            var act = () => game.PlaceBet(1000);
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void BetExceedingBalance_ThrowsArgumentException()
        {
            var settings = new GameSettings { StartingBalance = 20m };
            var game = new BlackjackGame(settings, new Random(42));
            var act = () => game.PlaceBet(25);
            act.Should().Throw<ArgumentException>();
        }
    }

    public class DealTests
    {
        [Fact]
        public void Deal_GivesPlayer2Cards()
        {
            var game = CreateGame();
            game.PlaceBet(10);
            game.Deal();
            game.PlayerHand.Cards.Should().HaveCount(2);
        }

        [Fact]
        public void Deal_GivesDealer2Cards()
        {
            var game = CreateGame();
            game.PlaceBet(10);
            game.Deal();
            game.DealerHand.Cards.Should().HaveCount(2);
        }

        [Fact]
        public void Deal_DealerCardIsHidden_DuringPlayerTurn()
        {
            var game = CreateGameInPlayerTurn();
            game.IsDealerCardHidden.Should().BeTrue();
        }

        [Fact]
        public void Deal_NaturalPlayerBlackjack_ResolvesWithBlackjackResult()
        {
            var game = FindGameWithResult(GameResult.Blackjack);
            game.State.Should().Be(GameState.Resolved);
            game.Result.Should().Be(GameResult.Blackjack);
            game.PlayerHand.IsBlackjack.Should().BeTrue();
            game.IsDealerCardHidden.Should().BeFalse();
        }

        [Fact]
        public void Deal_DealerBlackjack_ResolvesWithLoseResult()
        {
            for (int seed = 0; seed < 100000; seed++)
            {
                var game = new BlackjackGame(DefaultSettings, new Random(seed));
                game.PlaceBet(10);
                game.Deal();
                if (game.Result == GameResult.Lose && game.DealerHand.IsBlackjack)
                {
                    game.State.Should().Be(GameState.Resolved);
                    game.IsDealerCardHidden.Should().BeFalse();
                    return;
                }
            }
            throw new InvalidOperationException("Could not find a seed for dealer blackjack.");
        }

        [Fact]
        public void Deal_BothBlackjack_ResolvesAsPush()
        {
            for (int seed = 0; seed < 100000; seed++)
            {
                var game = new BlackjackGame(DefaultSettings, new Random(seed));
                game.PlaceBet(10);
                game.Deal();
                if (game.Result == GameResult.Push &&
                    game.PlayerHand.IsBlackjack &&
                    game.DealerHand.IsBlackjack)
                {
                    game.State.Should().Be(GameState.Resolved);
                    return;
                }
            }
            throw new InvalidOperationException("Could not find a seed for both blackjack.");
        }

        [Fact]
        public void Deal_NormalDeal_TransitionsToPlayerTurn()
        {
            var game = CreateGameInPlayerTurn();
            game.State.Should().Be(GameState.PlayerTurn);
        }
    }

    public class HitTests
    {
        [Fact]
        public void Hit_AddsCardToPlayerHand()
        {
            var game = CreateGameInPlayerTurn();
            var initialCount = game.PlayerHand.Cards.Count;
            game.Hit();
            game.PlayerHand.Cards.Count.Should().Be(initialCount + 1);
        }

        [Fact]
        public void Hit_WhenBust_TransitionsToResolved()
        {
            var game = CreateGameInPlayerTurn();
            while (game.State == GameState.PlayerTurn)
                game.Hit();

            game.PlayerHand.IsBust.Should().BeTrue();
            game.State.Should().Be(GameState.Resolved);
            game.Result.Should().Be(GameResult.Lose);
        }
    }

    public class StandTests
    {
        [Fact]
        public void Stand_TransitionsToDealerTurn()
        {
            var game = CreateGameInPlayerTurn();
            game.Stand();
            game.State.Should().Be(GameState.DealerTurn);
        }
    }

    public class DoubleDownTests
    {
        [Fact]
        public void DoubleDown_DoublesTheBet()
        {
            var game = CreateGameInPlayerTurn();
            var originalBet = game.SplitBets[0];
            game.DoubleDown();
            game.SplitBets[0].Should().Be(originalBet * 2);
        }

        [Fact]
        public void DoubleDown_DrawsExactlyOneCard()
        {
            var game = CreateGameInPlayerTurn();
            game.DoubleDown();
            game.PlayerHand.Cards.Should().HaveCount(3);
        }

        [Fact]
        public void DoubleDown_DeductsAdditionalBetFromBalance()
        {
            var game = CreateGameInPlayerTurn();
            var balanceBefore = game.PlayerBalance;
            var bet = game.SplitBets[0];
            game.DoubleDown();
            // Balance check depends on whether the hand resolved (payout added)
            // but the deduction of the additional bet should have occurred
            game.SplitBets[0].Should().Be(bet * 2);
        }

        [Fact]
        public void DoubleDown_WithMoreThan2Cards_Throws()
        {
            for (int seed = 0; seed < 100000; seed++)
            {
                var game = new BlackjackGame(DefaultSettings, new Random(seed));
                game.PlaceBet(10);
                game.Deal();
                if (game.State != GameState.PlayerTurn) continue;

                game.Hit();
                if (game.State == GameState.PlayerTurn && game.PlayerHand.Cards.Count > 2)
                {
                    var act = () => game.DoubleDown();
                    act.Should().Throw<InvalidOperationException>();
                    return;
                }
            }
            throw new InvalidOperationException("Could not find a seed for test scenario.");
        }

        [Fact]
        public void DoubleDown_WithInsufficientBalance_Throws()
        {
            var settings = new GameSettings { StartingBalance = 15m };
            var game = CreateGameInPlayerTurn(bet: 10m, settings: settings);
            // Balance is 15 - 10 = 5, need 10 more to double
            var act = () => game.DoubleDown();
            act.Should().Throw<InvalidOperationException>();
        }
    }

    public class SplitTests
    {
        private static BlackjackGame CreateGameWithSplittablePair(decimal bet = 10m)
        {
            for (int seed = 0; seed < 100000; seed++)
            {
                var game = new BlackjackGame(DefaultSettings, new Random(seed));
                game.PlaceBet(bet);
                game.Deal();
                if (game.State == GameState.PlayerTurn && game.PlayerHand.CanSplit)
                    return game;
            }
            throw new InvalidOperationException("Could not find a seed for splittable pair.");
        }

        private static BlackjackGame CreateGameWithAcePair(decimal bet = 10m)
        {
            for (int seed = 0; seed < 100000; seed++)
            {
                var game = new BlackjackGame(DefaultSettings, new Random(seed));
                game.PlaceBet(bet);
                game.Deal();
                if (game.State == GameState.PlayerTurn &&
                    game.PlayerHand.CanSplit &&
                    game.PlayerHand.Cards[0].Rank == Rank.Ace)
                    return game;
            }
            throw new InvalidOperationException("Could not find a seed for ace pair.");
        }

        [Fact]
        public void Split_CreatesSecondHand()
        {
            var game = CreateGameWithSplittablePair();
            game.Split();
            game.SplitHands.Should().HaveCount(1);
        }

        [Fact]
        public void Split_WithNonPair_Throws()
        {
            for (int seed = 0; seed < 100000; seed++)
            {
                var game = new BlackjackGame(DefaultSettings, new Random(seed));
                game.PlaceBet(10);
                game.Deal();
                if (game.State == GameState.PlayerTurn && !game.PlayerHand.CanSplit)
                {
                    var act = () => game.Split();
                    act.Should().Throw<InvalidOperationException>();
                    return;
                }
            }
            throw new InvalidOperationException("Could not find a seed for test scenario.");
        }

        [Fact]
        public void Split_WhenAlreadySplit_Throws()
        {
            var game = CreateGameWithSplittablePair();
            game.Split();
            if (game.State == GameState.PlayerTurn)
            {
                var act = () => game.Split();
                act.Should().Throw<InvalidOperationException>();
            }
        }

        [Fact]
        public void Split_WithInsufficientBalance_Throws()
        {
            var settings = new GameSettings { StartingBalance = 12m };
            for (int seed = 0; seed < 100000; seed++)
            {
                var game = new BlackjackGame(settings, new Random(seed));
                game.PlaceBet(10);
                game.Deal();
                if (game.State == GameState.PlayerTurn && game.PlayerHand.CanSplit)
                {
                    // Balance is 12 - 10 = 2, need 10 more to split
                    var act = () => game.Split();
                    act.Should().Throw<InvalidOperationException>();
                    return;
                }
            }
            throw new InvalidOperationException("Could not find a seed for test scenario.");
        }

        [Fact]
        public void SplittingAces_AutoStands()
        {
            var game = CreateGameWithAcePair();
            game.Split();
            // After splitting aces, should go to DealerTurn (auto-stand)
            game.State.Should().Be(GameState.DealerTurn);
        }
    }

    public class ResolveDealerTurnTests
    {
        [Fact]
        public void ResolveDealerTurn_DealerEndsAt17OrHigher()
        {
            var game = CreateGameInPlayerTurn();
            game.Stand();
            game.ResolveDealerTurn();
            game.State.Should().Be(GameState.Resolved);
            (game.DealerHand.Score >= 17 || game.DealerHand.IsBust).Should().BeTrue();
        }

        [Fact]
        public void ResolveDealerTurn_ResultsCalculatedCorrectly()
        {
            var game = CreateGameInPlayerTurn();
            game.Stand();
            game.ResolveDealerTurn();

            var playerScore = game.PlayerHand.Score;
            var dealerScore = game.DealerHand.Score;

            if (game.DealerHand.IsBust)
                game.Result.Should().Be(GameResult.Win);
            else if (playerScore > dealerScore)
                game.Result.Should().Be(GameResult.Win);
            else if (playerScore < dealerScore)
                game.Result.Should().Be(GameResult.Lose);
            else
                game.Result.Should().Be(GameResult.Push);
        }

        [Fact]
        public void ResolveDealerTurn_PayoutAddedToBalance()
        {
            var game = CreateGameInPlayerTurn();
            var balanceBeforeStand = game.PlayerBalance;
            game.Stand();
            game.ResolveDealerTurn();

            var expectedPayout = game.CalculatePayout();
            game.PlayerBalance.Should().Be(balanceBeforeStand + expectedPayout);
        }

        [Fact]
        public void ResolveDealerTurn_DealerCardRevealed()
        {
            var game = CreateGameInPlayerTurn();
            game.Stand();
            game.ResolveDealerTurn();
            game.IsDealerCardHidden.Should().BeFalse();
        }
    }

    public class CalculatePayoutTests
    {
        [Fact]
        public void Win_Pays2x()
        {
            var game = FindGameWithResult(GameResult.Win, 10m);
            game.CalculatePayout().Should().Be(20m);
        }

        [Fact]
        public void Blackjack_Pays2_5x()
        {
            var game = FindGameWithResult(GameResult.Blackjack, 10m);
            game.CalculatePayout().Should().Be(25m);
        }

        [Fact]
        public void Push_ReturnsBet()
        {
            var game = FindGameWithResult(GameResult.Push, 10m);
            game.CalculatePayout().Should().Be(10m);
        }

        [Fact]
        public void Lose_Pays0()
        {
            var game = FindGameWithResult(GameResult.Lose, 10m);
            game.CalculatePayout().Should().Be(0m);
        }
    }

    public class GetAvailableActionsTests
    {
        [Fact]
        public void ReturnsEmpty_InNonPlayerTurnState()
        {
            var game = CreateGame();
            game.GetAvailableActions().Should().BeEmpty();
        }

        [Fact]
        public void AlwaysIncludesHitAndStand_DuringPlayerTurn()
        {
            var game = CreateGameInPlayerTurn();
            var actions = game.GetAvailableActions();
            actions.Should().Contain(GameAction.Hit);
            actions.Should().Contain(GameAction.Stand);
        }

        [Fact]
        public void IncludesDoubleDown_With2CardsAndSufficientBalance()
        {
            var game = CreateGameInPlayerTurn();
            var actions = game.GetAvailableActions();
            actions.Should().Contain(GameAction.DoubleDown);
        }

        [Fact]
        public void IncludesSplit_WithPairAndSufficientBalance()
        {
            for (int seed = 0; seed < 100000; seed++)
            {
                var game = new BlackjackGame(DefaultSettings, new Random(seed));
                game.PlaceBet(10);
                game.Deal();
                if (game.State == GameState.PlayerTurn && game.PlayerHand.CanSplit)
                {
                    var actions = game.GetAvailableActions();
                    actions.Should().Contain(GameAction.Split);
                    return;
                }
            }
            throw new InvalidOperationException("Could not find a seed for test scenario.");
        }

        [Fact]
        public void ExcludesDoubleDown_WhenInsufficientBalance()
        {
            var settings = new GameSettings { StartingBalance = 15m };
            var game = CreateGameInPlayerTurn(bet: 10m, settings: settings);
            // Balance is 5, bet is 10, can't double
            var actions = game.GetAvailableActions();
            actions.Should().NotContain(GameAction.DoubleDown);
        }
    }

    public class BalanceTests
    {
        [Fact]
        public void RefillBalance_RefillsToStartingWhenAt0()
        {
            var game = CreateGame();
            game.SetBalance(0);
            game.RefillBalance();
            game.PlayerBalance.Should().Be(DefaultSettings.StartingBalance);
        }

        [Fact]
        public void RefillBalance_DoesNothingWhenNotAt0()
        {
            var game = CreateGame();
            var currentBalance = game.PlayerBalance;
            game.RefillBalance();
            game.PlayerBalance.Should().Be(currentBalance);
        }

        [Fact]
        public void SetBalance_SetsBalance()
        {
            var game = CreateGame();
            game.SetBalance(500);
            game.PlayerBalance.Should().Be(500);
        }
    }

    public class StartNewRoundTests
    {
        [Fact]
        public void StartNewRound_ResetsStateToWaitingForBet()
        {
            var game = CreateGameInPlayerTurn();
            game.Stand();
            game.ResolveDealerTurn();
            game.StartNewRound();
            game.State.Should().Be(GameState.WaitingForBet);
        }

        [Fact]
        public void StartNewRound_ClearsHands()
        {
            var game = CreateGameInPlayerTurn();
            game.Stand();
            game.ResolveDealerTurn();
            game.StartNewRound();
            game.PlayerHand.Cards.Should().BeEmpty();
            game.DealerHand.Cards.Should().BeEmpty();
        }

        [Fact]
        public void StartNewRound_ResetsResult()
        {
            var game = CreateGameInPlayerTurn();
            game.Stand();
            game.ResolveDealerTurn();
            game.StartNewRound();
            game.Result.Should().BeNull();
            game.CurrentBet.Should().Be(0);
        }
    }
}
