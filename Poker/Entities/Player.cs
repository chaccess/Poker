using Poker.Services;
using Poker.Services.CombinationCalculator;

namespace Poker.Entities
{
    public class Player(string name) : BaseEntity
    {
        public string Name { get; set; } = name;

        public List<Card> Hand { get; set; } = [];

        public int Bank { get; set; }

        public int SeatNumber { get; set; }

        public PlayerPosition? Position { get; set; }

        public BettingState BettingState { get; set; }

        public CombinationResult CombinationResult { get; set; } = new CombinationResult(CombinationType.None, []);
    }
}
