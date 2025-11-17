using Poker.Services.BettingMechanism;
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
            AvailableActions = [];
        }

        public string Name { get; set; }

        public List<Card> Hand { get; set; } = [];

        public int Bank { get; set; }

        public int SeatNumber { get; set; }

        public PlayerPosition? Position { get; set; }

        public PlayerBettingState? BettingState { get; set; }

        public CombinationResult CombinationResult { get; set; } = new CombinationResult(CombinationType.None, []);

        public List<PlayerAction> AvailableActions
        {
            get;
            set;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public enum PlayerAction
    {
        Call,
        Raise,
        AllIn,
        Check,
        Fold
    }
}
