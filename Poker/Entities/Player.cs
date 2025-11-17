using Poker.Services.BettingService;
using Poker.Services.CombinationService;
using Poker.Structs;

namespace Poker.Entities
{
    public class Player : BaseEntity
    {
        public Player(string name)
        {
            Id = Guid.NewGuid();
            Name = name;
            Bank = 5000;
        }

        public string Name { get; set; }

        public List<Card> Hand { get; set; } = [];

        public int Bank { get; set; }

        public int SeatNumber { get; set; }

        public PlayerPosition? Position { get; set; }

        public PlayerBettingState? BettingState { get; set; }

        public CombinationResult CombinationResult { get; set; } = new CombinationResult(CombinationType.None, []);

        public override string ToString()
        {
            return Name;
        }
    }
}
