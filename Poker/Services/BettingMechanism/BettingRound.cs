using Poker.Entities;
using System.ComponentModel;
using System.Numerics;

namespace Poker.Services.BettingService
{
    public class BettingRound : INotifyPropertyChanged
    {
        private List<Player> _activePlayers;
        private int _currentPlayerIndex;
        private BettingTrigger _currenTtrigger;
        private int? _currentBet;
        private int _lastBet;
        private Player? _lastRaiser;
        private readonly Dictionary<(BettingState, BettingTrigger), (BettingState next, Action? onTransition)> _transitions = [];
        private BettingState _state = BettingState.WaitingForPlayer;
        private readonly Dictionary<Player, int> _bets;

        public BettingRound()
        {
            _activePlayers = [];
            _bets = [];
            ConfigureTransitions();
        }

        public Player CurrentPlayer => _activePlayers[_currentPlayerIndex];
        public BettingState State
        {
            get => _state;
            set
            {
                if (_state == value) return;
                _state = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(State)));
            }
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        public Dictionary<Player, int> Bets => _bets;
        public int LastBet
        {
            get => _lastBet;
            set
            {
                if (_lastBet == value) return;
                _lastBet = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LastBet)));
            }
        }
        public BettingRoundType RoundType { get; set; }

        public void Setup(List<Player> players, BettingRoundType roundType)
        {
            _activePlayers = players.OrderBy(x => x.Position).ToList();
            _currentPlayerIndex = _activePlayers.IndexOf(_activePlayers.First(x => x.Position == PlayerPosition.SB));
            RoundType = roundType;
            LastBet = 0;
            _lastRaiser = null;
            State = BettingState.WaitingForPlayer;
            Bets.Clear();
        }

        private void ConfigureTransitions()
        {
            // Игрок сделал действие => ждём следующего
            Permit(BettingState.WaitingForPlayer, BettingTrigger.Call, BettingState.PlayerActed, OnPlayerAction);
            Permit(BettingState.WaitingForPlayer, BettingTrigger.Raise, BettingState.PlayerActed, OnPlayerAction);
            Permit(BettingState.WaitingForPlayer, BettingTrigger.Check, BettingState.PlayerActed, OnPlayerAction);
            Permit(BettingState.WaitingForPlayer, BettingTrigger.Fold, BettingState.PlayerActed, OnPlayerAction);
            Permit(BettingState.WaitingForPlayer, BettingTrigger.AllIn, BettingState.PlayerActed, OnPlayerAction);

            // После действия — переход к следующему игроку
            Permit(BettingState.PlayerActed, BettingTrigger.NextPlayer, BettingState.WaitingForPlayer, MoveToNextPlayer);

            // Завершение круга
            Permit(BettingState.PlayerActed, BettingTrigger.RoundEnd, BettingState.RoundComplete, OnRoundComplete);
        }

        private void Permit(BettingState from, BettingTrigger trigger, BettingState to, Action? onTransition = null)
        {
            _transitions[(from, trigger)] = (to, onTransition);
        }

        public void Fire(BettingTrigger trigger, int? currentBet = null)
        {
            if (_transitions.TryGetValue((_state, trigger), out var info))
            {
                Console.WriteLine($"{_state} -> {info.next} ({trigger})");
                _currenTtrigger = trigger;
                _currentBet = currentBet;
                info.onTransition?.Invoke();
                State = info.next;
            }
            else
            {
                Console.WriteLine($"Invalid transition: {_state} + {trigger}");
                throw new InvalidOperationException($"Invalid transition: {_state} + {trigger}");
            }
        }

        public void Blind(Player player, int bet)
        {
            if (player.Position != PlayerPosition.SB && player.Position != PlayerPosition.BB)
            {
                throw new InvalidOperationException("Only players with BB and SB positions can set this type of bet");
            }

            _bets.Add(player, bet);
            player.BettingState = player.Position == PlayerPosition.BB ? PlayerBettingState.BBlind : PlayerBettingState.SBlind;
            LastBet = bet > _lastBet ? bet : _lastBet;
            Console.WriteLine($"{CurrentPlayer.Name} blind: {_lastBet}");
            MoveToNextPlayer();
        }

        private void OnPlayerAction()
        {
            Console.WriteLine($"{CurrentPlayer.Name} made an action.");
            Action action = _currenTtrigger switch
            {
                BettingTrigger.Call => Call,
                BettingTrigger.Raise => Raise,
                BettingTrigger.AllIn => AllIn,
                BettingTrigger.Check => Check,
                BettingTrigger.Fold => FoldCards,
                _ => throw new NotImplementedException(),
            };

            action.Invoke();
        }

        private void Call()
        {
            if (LastBet == 0) throw new ApplicationException("Need to get blinds first");

            if (CurrentPlayer.Bank == 0)
            {
                throw new InvalidOperationException("Player has 0 money");
            }
            else if (CurrentPlayer.Bank <= LastBet)
            {
                AllIn();
                return;
            }

            if (_bets.TryGetValue(CurrentPlayer, out int value))
            {
                _bets[CurrentPlayer] += LastBet - value;
                CurrentPlayer.Bank -= LastBet - value;
            }
            else
            {
                _bets.Add(CurrentPlayer, LastBet);
                CurrentPlayer.Bank -= LastBet;
            }
            CurrentPlayer.BettingState = PlayerBettingState.Call;
            Console.WriteLine($"{CurrentPlayer.Name} called {_lastBet}");
        }

        private void Raise()
        {
            var bet = _currentBet ?? 0;

            if (CurrentPlayer.Bank == 0)
            {
                throw new InvalidOperationException("Player has 0 money");
            }
            else if (CurrentPlayer.Bank <= _currentBet)
            {
                AllIn();
                return;
            }

            if (_bets.TryGetValue(CurrentPlayer, out int value))
            {
                _bets[CurrentPlayer] = value + bet;
                CurrentPlayer.Bank -= bet;
            }
            else
            {
                _bets.Add(CurrentPlayer, bet);
                CurrentPlayer.Bank -= bet;
            }
            CurrentPlayer.BettingState = PlayerBettingState.Raise;
            _lastRaiser = CurrentPlayer;
            LastBet = bet > _lastBet ? bet : _lastBet;
            Console.WriteLine($"{CurrentPlayer.Name} raised {bet}");
        }

        private void AllIn()
        {
            var bet = CurrentPlayer.Bank;
            if (_bets.TryGetValue(CurrentPlayer, out int value))
            {
                _bets[CurrentPlayer] = value + bet;
                CurrentPlayer.Bank = 0;
            }
            else
            {
                _bets.Add(CurrentPlayer, bet);
                CurrentPlayer.Bank = 0;
            }
            CurrentPlayer.BettingState = PlayerBettingState.AllIn;
            LastBet = bet > _lastBet ? bet : _lastBet;
            Console.WriteLine($"{CurrentPlayer.Name} allInned with {bet}");
        }

        private void Check()
        {
            if (_bets.Count == 0 || _bets[CurrentPlayer] == _lastBet)
            {
                CurrentPlayer.BettingState = PlayerBettingState.Check;
                Console.WriteLine($"{CurrentPlayer.Name} checked");
                return;
            }

            throw new InvalidOperationException("Player can't CHECK at this betting round state");
        }

        private void FoldCards()
        {
            CurrentPlayer.Hand.Clear();
            CurrentPlayer.BettingState = PlayerBettingState.Fold;
            Console.WriteLine($"{CurrentPlayer.Name} folded");
        }

        private void MoveToNextPlayer()
        {
            _currentPlayerIndex = (_currentPlayerIndex + 1) % _activePlayers.Count;

            if (AllPlayersActed())
                Fire(BettingTrigger.RoundEnd);
            else
            {
                CurrentPlayer.BettingState = PlayerBettingState.Thinking;
                Console.WriteLine($"Next player: {CurrentPlayer.Name}");
            }
        }

        private void OnRoundComplete()
        {
            // здесь будет логика логирования и что-то еще
            Console.WriteLine("Betting round complete!");
        }

        private bool AllPlayersActed()
        {
            // 1. Нет игроков в состоянии "Thinking"
            if (_activePlayers.Any(p => p.BettingState == PlayerBettingState.Thinking))
                return false;

            // 2. На префлопе биг блайн должен сказать слово.
            if (_activePlayers.Where(p => p.BettingState == PlayerBettingState.BBlind).Any())
            {
                return false;
            }

            // 3. Проверяем, выровнены ли ставки
            var active_bets = _activePlayers
                .Where(p => p.BettingState != PlayerBettingState.Fold)
                .Select(p => _bets.ContainsKey(p) ? _bets[p] : 0)
                .ToList();

            int maxBet = active_bets.Max();

            bool allEqual = _activePlayers
                .Where(p => p.BettingState != PlayerBettingState.Fold)
                .All(p =>
                {
                    int bet = _bets.TryGetValue(p, out int v) ? v : 0;
                    return bet == maxBet || p.BettingState == PlayerBettingState.AllIn;
                });

            if (!allEqual)
                return false;

            // 4. Если был рейз — надо дождаться, пока круг вернётся к последнему рейзеру
            if (_lastRaiser != null && CurrentPlayer != _lastRaiser)
                return false;

            return true;
        }


        public int GetBankAmount()
        {
            var res = 0;

            foreach (var bet in _bets)
            {
                res += bet.Value;
            }

            return res;
        }
    }

    public enum BettingState
    {
        WaitingForPlayer,   // ожидаем действие от текущего игрока
        PlayerActed,        // игрок сделал ход, ждём следующего
        RoundComplete       // круг ставок завершён
    }

    public enum BettingTrigger
    {
        Call,
        Raise,
        Check,
        Fold,
        AllIn,
        NextPlayer,
        RoundEnd
    }

    public enum BettingRoundType
    {
        PreflopRound,
        FlopRound,
        TurnRound,
        RiverRound
    }
}
