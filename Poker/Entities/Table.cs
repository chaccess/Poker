using Poker.Extentions;
using Poker.Services.BettingService;
using Poker.Services.CombinationService;
using Poker.Structs;

namespace Poker.Entities
{
    public class Table : BaseEntity
    {
        private readonly object _lock = new();
        private GameState GameState;
        private readonly Dictionary<(GameState, GameStateTrigger), (GameState next, Action? onTransition)> _transitions;
        private readonly BettingMechanism _bettingMechanism;
        private readonly CombinationService _combinationService;
        private readonly Croupier _croupier;
        private readonly List<Player> _activePlayers;
        private readonly List<Player> _players;
        private readonly List<Card> _desc;
        private int _bank;
        private List<Player> _winners;
        private int _lastSBSeat;
        private Blinds _blinds;
        private int _lastBet;

        public Table(Blinds startBlinds, TableStatus status, GameState? initialState = null)
        {
            _croupier = new Croupier("John");
            _combinationService = new CombinationService();
            _bettingMechanism = new BettingMechanism();
            _bettingMechanism.PropertyChanged += OnBettingMechanism_PropertyChanged;
            _blinds = startBlinds;
            _players = [];
            _activePlayers = [];
            _desc = [];
            _winners = [];
            GameState = initialState ?? GameState.WaitingForPlayers;
            Status = status;
            _transitions = [];
            _lastSBSeat = 0;
            ConfigureTransitions();
        }

        public static readonly int MaxPlayers = 6;
        public TableStatus Status { get; set; }
        public Croupier Croupier => _croupier;
        public List<Player> Players => _players;
        public List<Player> ActivePlayers => _activePlayers;
        public List<Card> Desc => _desc;
        public int Bank => _bank;
        public List<Player> Winners => _winners;
        public Blinds Blinds => _blinds;
        public int LastBet => _lastBet;

        public void Permit(GameState from, GameStateTrigger trigger, GameState to, Action? onTransition = null)
        {
            _transitions[(from, trigger)] = (to, onTransition);
        }

        public void Fire(GameStateTrigger trigger)
        {
            if (_transitions.TryGetValue((GameState, trigger), out var info))
            {
                Console.WriteLine($"Transition: {GameState} => {info.next}");
                info.onTransition?.Invoke();
                GameState = info.next;
            }
            else
            {
                Console.WriteLine($"Unavalable transition: {GameState} + {trigger}");
            }
        }

        private void ConfigureTransitions()
        {
            Permit(GameState.WaitingForPlayers, GameStateTrigger.Init, GameState.Initialized, Init);
            Permit(GameState.Initialized, GameStateTrigger.StartDealHands, GameState.DealHands, DealHands);
            Permit(GameState.DealHands, GameStateTrigger.StartPreflopBetting, GameState.PreflopBetting, StartBettingRound);
            Permit(GameState.PreflopBetting, GameStateTrigger.StartDealFlop, GameState.DealFlop, DealFlop);
            Permit(GameState.DealFlop, GameStateTrigger.StartFlopBetting, GameState.FlopBetting, StartBettingRound);
            Permit(GameState.FlopBetting, GameStateTrigger.SartDealTurn, GameState.DealTurn, DealTurn);
            Permit(GameState.DealTurn, GameStateTrigger.StartTurnBetting, GameState.TurnBetting, StartBettingRound);
            Permit(GameState.TurnBetting, GameStateTrigger.StartDealRiver, GameState.DealRiver, DealRiver);
            Permit(GameState.DealRiver, GameStateTrigger.StartRiverBetting, GameState.RiverBetting, StartBettingRound);
            Permit(GameState.RiverBetting, GameStateTrigger.StartShowDown, GameState.Showdown, Showdown);
            Permit(GameState.Showdown, GameStateTrigger.StartEvaluateHands, GameState.EvaluateHands, EvaluateHands);
            Permit(GameState.EvaluateHands, GameStateTrigger.StartPayout, GameState.Payout, Payout);
            Permit(GameState.Payout, GameStateTrigger.EndGame, GameState.GameEnded, EndGame);
            Permit(GameState.GameEnded, GameStateTrigger.Reset, GameState.WaitingForPlayers, Reset);
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
                if (ActivePlayers.Where(x => x.SeatNumber == seatNumber).Any())
                {
                    var playerToRemove = ActivePlayers.Where(p => p.SeatNumber == seatNumber).FirstOrDefault();

                    if (playerToRemove != null)
                    {
                        ActivePlayers.Remove(playerToRemove);
                    }
                }

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

        public Player GetCurrentPlayer()
        {
            return _bettingMechanism.GetCurrentPlayer();
        }

        private void Init()
        {
            _desc.Clear();
            _croupier.Reset();
            _bank = 0;
            _winners.Clear();
            _activePlayers.Clear();
            _activePlayers.AddRange(_players.Clone());
            ResetPlayers();
            _bettingMechanism.Configure(_players, BettingRoundType.PreflopRound, Blinds);
        }

        private void ResetPlayers()
        {
            lock (_lock)
            {
                if (ActivePlayers.Count < 2)
                {
                    foreach (var player in ActivePlayers)
                    {
                        player.BettingState = null;
                        player.CombinationResult = new(CombinationType.None, []);
                        player.Hand.Clear();
                    }
                    return;
                }

                var seatOrder = ActivePlayers.OrderBy(x => x.SeatNumber).ToList();
                var newSBPlayer = seatOrder.First(x => x.SeatNumber > _lastSBSeat);
                var firstIndex = Array.IndexOf(seatOrder.ToArray(), newSBPlayer);
                _lastSBSeat = firstIndex;

                var newOrder = new List<Player>();

                for (int i = 0; i < seatOrder.Count; i++)
                {
                    newOrder.Add(seatOrder[firstIndex++]);

                    if (firstIndex == seatOrder.Count)
                    {
                        firstIndex = 0;
                    }
                }

                var c = 0;
                var orderList = GetPositionsOrder();
                foreach (var player in newOrder)
                {
                    player.Position = orderList[c++];
                    player.BettingState = null;
                    player.CombinationResult = new(CombinationType.None, []);
                    player.Hand.Clear();
                }
            }
        }

        private void DealHands()
        {
            lock (_lock)
            {
                Croupier.DealStartHands(ActivePlayers);
            }
        }

        private void StartBettingRound()
        {
            _bettingMechanism.SetGameState((GameState)(int)GameState + 1);
        }

        private void OnBettingMechanism_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs eventArgs)
        {
            if (eventArgs.PropertyName == nameof(_bettingMechanism.RoundEnded) && _bettingMechanism.RoundEnded)
            {
                _bank = _bettingMechanism.TotalBank;
            }

            if (eventArgs.PropertyName == nameof(_bettingMechanism.LastBet))
            {
                _lastBet = _bettingMechanism.LastBet;
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
            var active = ActivePlayers.Where(x => x.BettingState != PlayerBettingState.Fold);
            foreach (var player in active)
            {
                player.CombinationResult = _combinationService.GetCombination(player.Hand, Desc);
            }
        }

        private void EvaluateHands()
        {
            _winners = Croupier.GetWinner(ActivePlayers.Where(x => x.BettingState != PlayerBettingState.Fold).ToList(), Desc);
        }

        private void Payout()
        {
            _bettingMechanism.GetPrize(Winners);
            _bank = 0;
        }

        private void EndGame()
        {
            // пока не знаю, нужно ли тут что-то(логирование)
        }

        private void Reset()
        {
        }

        public void MakePlayerAction(Guid playerId, BettingTrigger bettingTrigger, int bet = 0)
        {
            var actingPlayer = _activePlayers.FirstOrDefault(x => x.Id == playerId) ?? throw new MakePlayerActionException($"No players with id = {playerId} found.");
            try
            {
                if (bettingTrigger == BettingTrigger.Call)
                {
                    _bettingMechanism.Call(actingPlayer);
                }
                else if (bettingTrigger == BettingTrigger.Raise)
                {
                    _bettingMechanism.Raise(actingPlayer, bet);
                }
                else if (bettingTrigger == BettingTrigger.AllIn)
                {
                    _bettingMechanism.AllIn(actingPlayer);
                }
                else if (bettingTrigger == BettingTrigger.Check)
                {
                    _bettingMechanism.Check(actingPlayer);
                }
                else if (bettingTrigger == BettingTrigger.Fold)
                {
                    _bettingMechanism.Fold(actingPlayer);
                }
            }
            catch (NullReferenceException nre)
            {
                throw new MakePlayerActionException(nre.Message);
            }
            catch (InvalidOperationException ioe)
            {
                throw new MakePlayerActionException(ioe.Message);
            }
            catch (Exception e)
            {
                throw new MakePlayerActionException(e.Message);
            }
        }

        private List<PlayerPosition> GetPositionsOrder()
        {
            lock (_lock)
            {
                return ActivePlayers.Count switch
                {
                    2 => [PlayerPosition.SB, PlayerPosition.BB],
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
        WaitingForPlayers,
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
        EndGame,
        Reset,
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

    public class MakePlayerActionException : Exception
    {
        public MakePlayerActionException() : base()
        {
        }

        public MakePlayerActionException(string? message) : base(message)
        {
        }
    }
}
