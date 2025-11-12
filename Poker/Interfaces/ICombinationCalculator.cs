using Poker.Entities;
using Poker.Services.CombinationCalculator;

namespace Poker.Interfaces
{
    public interface ICombinationCalculator
    {
        public CombinationResult GetCombination(List<Card> hand, List<Card> desk);
    }
}
