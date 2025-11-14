using Poker.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Poker.Entities
{
    public class Table : BaseEntity
    {
        private readonly object _lock = new();
        private GameState ThisGameState;
        private readonly Dictionary<(GameState, GameStateTrigger), (GameState next, Action? onTransition)> _transitions;
        private readonly IBettingService _bettingService;

        public Table(TableStatus status, GameState initialState, IBettingService bettingService)
        {
            Croupier = new Croupier("John");
            Players = [];
            ThisGameState = initialState;
            Status = status;
            _transitions = [];
            ConfigureTransitions();
            _bettingService = bettingService;
        }

        public static readonly int MaxPlayers = 6;

        public TableStatus Status { get; set; }

        public required Croupier Croupier { get; set; }

        public required List<Player> Players { get; set; }

        public void Permit(GameState from, GameStateTrigger trigger, GameState to, Action? onTransition = null)
        {
            _transitions[(from, trigger)] = (to, onTransition);
        }

        public void Fire(GameStateTrigger trigger)
        {
            if (_transitions.TryGetValue((ThisGameState, trigger), out var info))
            {
                Console.WriteLine($"Переход: {ThisGameState} → {info.next}");
                info.onTransition?.Invoke();
                ThisGameState = info.next;
            }
            else
            {
                Console.WriteLine($"Недопустимый переход: {ThisGameState} + {trigger}");
            }
        }

        private void ConfigureTransitions()
        {
            Permit(GameState.Initializing, GameStateTrigger.InitCompleted, GameState.DealHands, Init);
        }

        public void AddPlayer(Player player, int seatNumber)
        {
            if (Players.Count < MaxPlayers)
            {
                lock (_lock)
                {
                    if (!Players.Contains(player) && !Players.Where(x => x.SeatNumber == seatNumber).Any())
                    {
                        Players.Add(player);
                        player.SeatNumber = seatNumber;
                    }
                }
            }
        }

        public void RemovePlayer(int seatNumber)
        {
            lock (_lock)
            {
                if (Players.Where(x => x.SeatNumber == seatNumber).Any())
                {
                    var playerToRemove = Players.Where(p => p.SeatNumber == seatNumber).FirstOrDefault();

                    if (playerToRemove != null)
                    {
                        Players.Remove(playerToRemove);
                    }
                }
            }
        }

        private void Init()
        {
            Croupier.Reset();
            CalculatePositions();
        }

        private void DealHands()
        {
            lock (_lock)
            {
                Croupier.DealStartHands(Players);
            }
        }

        private void CalculatePositions()
        {
            lock (_lock)
            {
                if (Players.Count == 2)
                {
                    (Players[1].Position, Players[0].Position) = (Players[0].Position, Players[1].Position);
                }

                var seatOrder = Players.OrderBy(x => x.SeatNumber).ToList();

                var newOrder = new List<Player>();

                var firstIndex = Array.IndexOf(seatOrder.ToArray(), seatOrder.Where(x => x.Position == PlayerPosition.BTN));

                firstIndex = firstIndex == -1 ? 0 : firstIndex;

                for (int i = 0; i < seatOrder.Count; i++)
                {
                    newOrder.Add(seatOrder[firstIndex++]);

                    if (firstIndex == seatOrder.Count - 1)
                    {
                        firstIndex = 0;
                    }
                }

                var c = 0;
                foreach (var player in newOrder)
                {
                    player.Position = GetPositionsOrder()[c++];
                }
            }
        }

        private List<PlayerPosition> GetPositionsOrder()
        {
            lock (_lock)
            {
                return Players.Count switch
                {
                    3 => [PlayerPosition.SB, PlayerPosition.BB, PlayerPosition.BTN],
                    4 => [PlayerPosition.SB, PlayerPosition.BB, PlayerPosition.CO, PlayerPosition.BTN],
                    5 => [PlayerPosition.SB, PlayerPosition.BB, PlayerPosition.UTG, PlayerPosition.CO, PlayerPosition.BTN],
                    6 => [PlayerPosition.SB, PlayerPosition.BB, PlayerPosition.UTG, PlayerPosition.MP, PlayerPosition.CO, PlayerPosition.BTN],
                    _ => throw new NotImplementedException(),
                };
            }
        }
    }

    public enum TableStatus
    {
        Normal,
        VIP
    }

    public enum GameState
    {
        Initializing,
        DealHands,
        PreflopBetting,
        DealFlop,
        FlopBetting,
        DealTurn,
        TurnBetting,
        DealRiver,
        RiverBetting,
        Showdown,
        EvaluateHands,
        Payout,
        Reset
    }

    public enum GameStateTrigger
    {
        InitCompleted,
        DealHandsCompleted,
        PreflopBettingCompleted,
        DealFlopCompleted,
        FlopBettingCompleted,
        DealTurnCompleted,
        TurnBettingCompleted,
        DealRiverCompleted,
        RiverBettingCompleted,
        ShowDownCompleted,
        EvaluateHandsCompleted,
        PayoutCompleted,
        ResetCompleted
    }

    public enum PlayerPosition
    {
        /// <summary>
        /// Small blind.
        /// </summary>
        SB = 1,
        /// <summary>
        /// Big blind.
        /// </summary>
        BB,
        /// <summary>
        /// Under the gun.
        /// </summary>
        UTG,
        /// <summary>
        /// Middle position.
        /// </summary>
        MP,
        /// <summary>
        /// Cutt-off.
        /// </summary>
        CO,
        /// <summary>
        /// Button.
        /// </summary>
        BTN
    }
}
