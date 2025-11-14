using Poker.Entities;
using Poker.Interfaces;

namespace Poker.Services.BettingService
{
    public class BettingService : IBettingService
    {
        private readonly BettingRound _bettingRound;

        public BettingService(List<Player> players, GameState gameState)
        {
            var roundType = gameState switch
            {
                GameState.PreflopBetting => BettingRoundType.PreflopRound,
                GameState.FlopBetting => BettingRoundType.FlopRound,
                GameState.TurnBetting => BettingRoundType.TurnRound,
                GameState.RiverBetting => BettingRoundType.RiverRound,
                _ => throw new NotImplementedException()
            };
            _bettingRound = new(players, roundType);
        }

        public int TotalBank { get; set; } = 0;

        public void Check(Player player)
        {
            _bettingRound.Fire(BettingTrigger.Check);
        }

        public void Call(Player player)
        {
            _bettingRound.Fire(BettingTrigger.Call);
        }

        public void Raise(Player player, int bet)
        {
            _bettingRound.Fire(BettingTrigger.Raise, bet);
        }

        public void AllIn(Player player)
        {
            _bettingRound.Fire(BettingTrigger.AllIn);
        }

        public void Fold(Player player)
        {
            _bettingRound.Fire(BettingTrigger.Fold);
        }

        public void GetPrize(List<Player> players)
        {
            foreach (Player player in players)
            {
                player.Bank += TotalBank / players.Count;
            }
        }
    }

    public enum PlayerBettingState
    {
        Thinking,
        BBlind,
        SBlind,
        Check,
        Call,
        Raise,
        AllIn,
        Fold,
    }
}
