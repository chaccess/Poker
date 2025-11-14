using Poker.Services.CombinationService;
using Poker.ValueObjects;

namespace Poker.Interfaces
{
    public interface ICombinationCalculator
    {
        public CombinationResult GetCombination(List<Card> hand, List<Card> desk);
    }
}
