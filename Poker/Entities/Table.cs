using Poker.Interfaces;
using Poker.Services.BettingService;
using Poker.Structs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Poker.Entities
{
    public class Table : BaseEntity
    {
        private readonly object _lock = new();
        private GameState GameState;
        private readonly Dictionary<(GameState, GameStateTrigger), (GameState next, Action? onTransition)> _transitions;
        private BettingService _bettingService = null!;
        private readonly Croupier _croupier;
        // разделить список игроков на активных и всех, что за столом
        private readonly List<Player> _players;
        private readonly List<Card> _desc;
        private int _bank;
        private List<Player> _winners;

        public Table(TableStatus status, GameState? initialState = null)
        {
            _croupier = new Croupier("John");
            _players = [];
            _desc = [];
            _winners = [];
            GameState = initialState ?? GameState.Initialized;
            Status = status;
            _transitions = [];
            ConfigureTransitions();
            Init();
        }

        public static readonly int MaxPlayers = 6;
        public TableStatus Status { get; set; }
        public Croupier Croupier => _croupier;
        public List<Player> Players => _players;
        public List<Card> Desc => _desc;
        public BettingService BettingService => _bettingService;
        public int Bank => _bank;
        public List<Player> Winners => _winners;

        public void Permit(GameState from, GameStateTrigger trigger, GameState to, Action? onTransition = null)
        {
            _transitions[(from, trigger)] = (to, onTransition);
        }

        public void Fire(GameStateTrigger trigger)
        {
            if (_transitions.TryGetValue((GameState, trigger), out var info))
            {
                Console.WriteLine($"Переход: {GameState} → {info.next}");
                info.onTransition?.Invoke();
                GameState = info.next;
            }
            else
            {
                Console.WriteLine($"Недопустимый переход: {GameState} + {trigger}");
            }
        }

        private void ConfigureTransitions()
        {
            Permit(GameState.GameEnded, GameStateTrigger.Init, GameState.Initialized, Init);
            Permit(GameState.Initialized, GameStateTrigger.StartDealHands, GameState.DealHands, DealHands);
            Permit(GameState.DealHands, GameStateTrigger.StartPreflopBetting, GameState.PreflopBetting, StartBettingRound);
            Permit(GameState.PreflopBetting, GameStateTrigger.StartDealFlop, GameState.DealFlop, DealFlop);
            Permit(GameState.DealFlop, GameStateTrigger.StartFlopBetting, GameState.FlopBetting, StartBettingRound);
            Permit(GameState.FlopBetting, GameStateTrigger.SartDealTurn, GameState.DealTurn, DealTurn);
            Permit(GameState.DealTurn, GameStateTrigger.StartTurnBetting, GameState.TurnBetting, StartBettingRound);
            Permit(GameState.TurnBetting, GameStateTrigger.StartDealRiver, GameState.DealRiver, DealRiver);
            Permit(GameState.DealRiver, GameStateTrigger.StartRiverBetting, GameState.RiverBetting, DealRiver);
            Permit(GameState.RiverBetting, GameStateTrigger.StartShowDown, GameState.Showdown, Showdown);
            Permit(GameState.Showdown, GameStateTrigger.StartEvaluateHands, GameState.EvaluateHands, EvaluateHands);
            Permit(GameState.EvaluateHands, GameStateTrigger.StartPayout, GameState.Payout, Payout);
            Permit(GameState.Payout, GameStateTrigger.EndGame, GameState.GameEnded, EndGame);
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
            _desc.Clear();
            Croupier.Reset();
            _bank = 0;
            _winners.Clear();
            _bettingService = null!;
            CalculatePositions();
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
                var orderList = GetPositionsOrder();
                foreach (var player in newOrder)
                {
                    player.Position = orderList[c++];
                }
            }
        }

        private void DealHands()
        {
            lock (_lock)
            {
                Croupier.DealStartHands(Players);
            }
        }

        private void StartBettingRound()
        {
            _bettingService = new BettingService(Players);
            _bettingService.SetGameState(GameState);
            _bettingService.PropertyChanged += OnBettingService_PropertyChanged;
        }

        private void OnBettingService_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs eventArgs)
        {
            if (eventArgs.PropertyName == nameof(BettingService.RoundEnded) && BettingService.RoundEnded)
            {
                _bank = BettingService.TotalBank;
            }
        }

        private void DealFlop()
        {
            _desc.AddRange(Croupier.DealFlop());
        }

        private void DealTurn()
        {
            _desc.Add(Croupier.DealTurn());
        }

        private void DealRiver()
        {
            _desc.Add(Croupier.DealRiver());
        }

        private void Showdown()
        {
            // пока не знаю, нужно ли тут что-то(логирование)
        }

        private void EvaluateHands()
        {
            _winners = Croupier.GetWinner(Players, Desc);
        }

        private void Payout()
        {
            _bettingService.GetPrize(Winners);
        }

        private void EndGame()
        {
            // пока не знаю, нужно ли тут что-то(логирование)
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
        Initialized,
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
        GameEnded,
    }

    public enum GameStateTrigger
    {
        Init,
        StartDealHands,
        StartPreflopBetting,
        StartDealFlop,
        StartFlopBetting,
        SartDealTurn,
        StartTurnBetting,
        StartDealRiver,
        StartRiverBetting,
        StartShowDown,
        StartEvaluateHands,
        StartPayout,
        EndGame
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
