using Poker.Services.CombinationService;
using Poker.Structs;

namespace Poker.Interfaces
{
    public interface ICombinationCalculator
    {
        public CombinationResult GetCombination(List<Card> hand, List<Card> desk);
    }
}
