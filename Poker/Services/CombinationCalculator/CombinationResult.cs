using Poker.Entities;

namespace Poker.Services.CombinationCalculator
{
    public class CombinationResult(CombinationType type, List<Card> combinationCards)
    {
        public CombinationType CombinationType { get; set; } = type;

        public List<Card> CombinationCards { get; set; } = combinationCards;
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
