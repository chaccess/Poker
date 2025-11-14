using Poker.Entities;
using System.Numerics;

namespace Poker.Services.BettingService
{
    public class BettingRound
    {
        private readonly List<Player> _activePlayers;
        private int _currentPlayerIndex = 0;
        private BettingTrigger _currenTtrigger;
        private int? _currentBet;
        private int _lastBet = 0;
        private Player? _lastRaiser;
        private readonly Dictionary<(BettingState, BettingTrigger), (BettingState next, Action? onTransition)> _transitions = [];
        private BettingState _state = BettingState.WaitingForPlayer;

        public BettingRound(List<Player> players, BettingRoundType roundType)
        {
            _activePlayers = players.OrderBy(x => x.Position).ToList();
            _currentPlayerIndex = players.IndexOf(_activePlayers.First(x => x.Position == PlayerPosition.BB)) + 1;
            RoundType = roundType;
            ConfigureTransitions();
        }

        public Player CurrentPlayer => _activePlayers[_currentPlayerIndex];
        public BettingState State => _state;
        public Dictionary<Player, int> Bets { get; set; } = [];
        public int LastBet => _lastBet;
        public BettingRoundType RoundType { get; set; }

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
                _state = info.next;
            }
            else
            {
                Console.WriteLine($"Invalid transition: {_state} + {trigger}");
            }
        }

        public void Blind(Player player, int bet, bool big)
        {
            Bets.Add(player, bet);
            player.BettingState = big ? PlayerBettingState.BBlind : PlayerBettingState.SBlind;
            _lastBet = bet > _lastBet ? bet : _lastBet;
        }

        private void OnPlayerAction()
        {
            Console.WriteLine($"{CurrentPlayer.Name} made an action.");
            Action action = _currenTtrigger switch
            {
                BettingTrigger.Call => Call,
                BettingTrigger.Raise => Raise,
                BettingTrigger.AllIn => AllIn,
                BettingTrigger.Check => MoveToNextPlayer,
                BettingTrigger.Fold => FoldCards,
                _ => throw new NotImplementedException(),
            };

            action.Invoke();
        }

        private void Call()
        {
            var lastBet = LastBet;

            if (lastBet == 0) throw new ApplicationException("Need to get blinds first");

            if (CurrentPlayer.Bank == 0)
            {
                throw new InvalidOperationException("Player has 0 money");
            }
            else if (CurrentPlayer.Bank <= lastBet)
            {
                AllIn();
                return;
            }

            if (Bets.TryGetValue(CurrentPlayer, out int value))
            {
                Bets[CurrentPlayer] = lastBet - value;
                CurrentPlayer.Bank -= lastBet - value;
            }
            else
            {
                Bets.Add(CurrentPlayer, lastBet);
                CurrentPlayer.Bank -= lastBet;
            }
            CurrentPlayer.BettingState = PlayerBettingState.Call;
            Fire(BettingTrigger.NextPlayer);
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

            if (Bets.TryGetValue(CurrentPlayer, out int value))
            {
                Bets[CurrentPlayer] = value + bet;
                CurrentPlayer.Bank -= bet;
            }
            else
            {
                Bets.Add(CurrentPlayer, bet);
                CurrentPlayer.Bank -= bet;
            }
            CurrentPlayer.BettingState = PlayerBettingState.Raise;
            _lastRaiser = CurrentPlayer;
            _lastBet = bet > _lastBet ? bet : _lastBet;
            Fire(BettingTrigger.NextPlayer);
        }

        private void AllIn()
        {
            var bet = CurrentPlayer.Bank;
            if (Bets.TryGetValue(CurrentPlayer, out int value))
            {
                Bets[CurrentPlayer] = value + bet;
                CurrentPlayer.Bank = 0;
            }
            else
            {
                Bets.Add(CurrentPlayer, bet);
                CurrentPlayer.Bank = 0;
            }
            CurrentPlayer.BettingState = PlayerBettingState.AllIn;
            _lastBet = bet > _lastBet ? bet : _lastBet;
            Fire(BettingTrigger.NextPlayer);
        }

        private void FoldCards()
        {
            CurrentPlayer.Hand.Clear();
            CurrentPlayer.BettingState = PlayerBettingState.Fold;
            Fire(BettingTrigger.NextPlayer);
        }

        private void MoveToNextPlayer()
        {
            _currentPlayerIndex = (_currentPlayerIndex + 1) % _activePlayers.Count;
            CurrentPlayer.BettingState = PlayerBettingState.Thinking;

            if (AllPlayersActed())
                Fire(BettingTrigger.RoundEnd);
            else
            {
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
            var activeBets = _activePlayers
                .Where(p => p.BettingState != PlayerBettingState.Fold)
                .Select(p => Bets.ContainsKey(p) ? Bets[p] : 0)
                .ToList();

            int maxBet = activeBets.Max();

            bool allEqual = _activePlayers
                .Where(p => p.BettingState != PlayerBettingState.Fold)
                .All(p =>
                {
                    int bet = Bets.TryGetValue(p, out int v) ? v : 0;
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

            foreach (var bet in Bets)
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
