using Poker.Services.BettingService;
using Poker.Services.CombinationService;
using Poker.ValueObjects;

namespace Poker.Entities
{
    public class Player(string name) : BaseEntity
    {
        public string Name { get; set; } = name;

        public List<Card> Hand { get; set; } = [];

        public int Bank { get; set; }

        public int SeatNumber { get; set; }

        public PlayerPosition? Position { get; set; }

        public PlayerBettingState BettingState { get; set; }

        public CombinationResult CombinationResult { get; set; } = new CombinationResult(CombinationType.None, []);
    }
}
