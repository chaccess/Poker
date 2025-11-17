using Poker.Services.CombinationService;
using Poker.Structs;

namespace Poker.Interfaces
{
    public interface ICombinationService
    {
        public CombinationResult GetCombination(List<Card> hand, List<Card> desk);
    }
}
