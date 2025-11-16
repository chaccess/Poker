using Poker.Structs;

namespace Poker.Services.CombinationService
{
    public class CombinationResult(CombinationType type, List<Card> combinationCards)
    {
        public CombinationType CombinationType { get; set; } = type;

        public List<Card> CombinationCards { get; set; } = combinationCards;

        public override string ToString()
        {
            return CombinationType.ToString();
        }
    }

    public enum CombinationType
    {
        None,
        Kicker,
        Pair,
        TwoPairs,
        Set,
        Straight,
        Flush,
        FullHouse,
        Quad,
        StraightFlush,
        RoyalFlush
    }
}
