using Poker.Entities;

namespace Poker.Interfaces
{
    public interface IBettingService
    {
        public void PassAction(Player player);

        public void Check(Player player);

        public void Call(Player player);

        public void Raise(Player player, int bet);

        public void AllIn(Player player);

        public void Fold(Player player);

        public void GetPrize(List<Player> players);
    }
}
