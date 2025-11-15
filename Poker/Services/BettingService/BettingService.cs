using Poker.Entities;
using Poker.Interfaces;
using System.ComponentModel;

namespace Poker.Services.BettingService
{
    public class BettingService : IBettingService
    {
        private BettingRound _bettingRound;
        private int _totalBank;
        private bool _roundEnded;
        private readonly List<Player> _players;

        public BettingService(List<Player> players)
        {
            _players = players;
            _bettingRound = new(players, BettingRoundType.PreflopRound);
            _bettingRound.PropertyChanged += OnBettingRound_PropertyChanged;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public int TotalBank
        {
            get => _totalBank;
            set
            {
                if (_totalBank == value) return;
                _totalBank = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalBank)));
            }
        }
        public bool RoundEnded
        {
            get => _roundEnded;
            set
            {
                if (_roundEnded == value) return;
                _roundEnded = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RoundEnded)));
            }
        }

        public void Check(Player player)
        {
            if (player == null) throw new NullReferenceException("Acting player can't be null");

            if (player.Id != _bettingRound.CurrentPlayer.Id) throw new InvalidOperationException("This player can't make actions.");

            _bettingRound.Fire(BettingTrigger.Check);
        }

        public void Call(Player player)
        {
            if (player == null) throw new NullReferenceException("Acting player can't be null");

            if (player.Id != _bettingRound.CurrentPlayer.Id) throw new InvalidOperationException("This player can't make actions.");

            _bettingRound.Fire(BettingTrigger.Call);
        }

        public void Raise(Player player, int bet)
        {
            if (player == null) throw new NullReferenceException("Acting player can't be null");

            if (player.Id != _bettingRound.CurrentPlayer.Id) throw new InvalidOperationException("This player can't make actions.");

            _bettingRound.Fire(BettingTrigger.Raise, bet);
        }

        public void AllIn(Player player)
        {
            if (player == null) throw new NullReferenceException("Acting player can't be null");

            if (player.Id != _bettingRound.CurrentPlayer.Id) throw new InvalidOperationException("This player can't make actions.");

            _bettingRound.Fire(BettingTrigger.AllIn);
        }

        public void Fold(Player player)
        {
            if (player == null) throw new NullReferenceException("Acting player can't be null");

            if (player.Id != _bettingRound.CurrentPlayer.Id) throw new InvalidOperationException("This player can't make actions.");

            _bettingRound.Fire(BettingTrigger.Fold);
        }

        public void SetGameState(GameState gameState)
        {
            var roundType = gameState switch
            {
                GameState.PreflopBetting => BettingRoundType.PreflopRound,
                GameState.FlopBetting => BettingRoundType.FlopRound,
                GameState.TurnBetting => BettingRoundType.TurnRound,
                GameState.RiverBetting => BettingRoundType.RiverRound,
                _ => throw new NotImplementedException()
            };

            _bettingRound = new(_players, roundType);
        }

        private void UpdateTotalBank()
        {
            _totalBank = _bettingRound.GetBankAmount();
        }

        public void GetPrize(List<Player> players)
        {
            foreach (Player player in players)
            {
                player.Bank += TotalBank / players.Count;
            }
        }

        private void OnBettingRound_PropertyChanged(object? sender, PropertyChangedEventArgs eventArgs)
        {
            if (eventArgs.PropertyName == nameof(_bettingRound.State) && _bettingRound.State == BettingState.RoundComplete)
            {
                UpdateTotalBank();
                _roundEnded = true;
            }
        }
    }

    public enum PlayerBettingState
    {
        Thinking,
        BBlind,
        SBlind,
        Check,
        Call,
        Raise,
        AllIn,
        Fold,
    }
}
