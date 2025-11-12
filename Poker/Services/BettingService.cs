using Poker.Entities;
using Poker.Interfaces;

namespace Poker.Services
{
    public class BettingService : IBettingService
    {
        public int TotalBank { get; set; } = 0;
        public Dictionary<Player, int> Bank { get; set; } = [];

        public void PassAction(Player player)
        {
            player.BettingState = BettingState.WaitingForDecision;
        }

        public void Check(Player player)
        {
            player.BettingState = BettingState.Check;
        }

        public void Call(Player player)
        {
            var lastBet = Bank.Last().Value;
            if (Bank.TryGetValue(player, out int value))
            {
                Bank[player] = value + lastBet;
                player.Bank -= lastBet;
            }
            else
            {
                Bank.Add(player, lastBet);
                player.Bank -= lastBet;
            }
            player.BettingState = BettingState.Raise;
        }

        public void Raise(Player player, int bet)
        {
            if (Bank.TryGetValue(player, out int value))
            {
                Bank[player] = value + bet;
                player.Bank -= bet;
            }
            else
            {
                Bank.Add(player, bet);
                player.Bank -= bet;
            }
            player.BettingState = BettingState.Raise;
        }

        public void AllIn(Player player)
        {
            Bank[player] += player.Bank;
            player.Bank = 0;
        }

        public void Fold(Player player)
        {
            player.BettingState = BettingState.Fold;
        }

        public void GetPrize(List<Player> players)
        {
            foreach (Player player in players)
            {
                player.Bank += TotalBank / players.Count;
            }
        }
    }

    public enum BettingState
    {
        WaitingForDecision,
        Check,
        Call,
        Raise,
        Fold,
    }
}
