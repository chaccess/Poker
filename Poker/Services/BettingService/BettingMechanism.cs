using Poker.Entities;
using Poker.Interfaces;
using Poker.Structs;
using System.ComponentModel;

namespace Poker.Services.BettingService
{
    public class BettingMechanism : IBettingMechanism
    {
        private BettingRound _bettingRound;
        private int _totalBank;
        private bool _roundEnded;
        private readonly List<Player> _players;
        private Blinds _blinds;
        private int _lastBet;

        public BettingMechanism()
        {
            _players = [];
            _bettingRound = new();
            _bettingRound.PropertyChanged += OnBettingRound_PropertyChanged;
        }

        public Blinds Blinds => _blinds;
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

        public void Configure(List<Player> players, BettingRoundType roundType, Blinds blinds)
        {
            _players.Clear();
            _players.AddRange(players);
            _bettingRound.Setup(players, roundType);
            _blinds = blinds;
        }

        public void SetBlinds(Player sb, Player bb)
        {
            if (sb == null || bb == null) throw new NullReferenceException("Acting player can't be null");

            if (sb.Id != _bettingRound.CurrentPlayer.Id) throw new InvalidOperationException("This player can't make actions.");

            _bettingRound.Blind(sb, _blinds.Small);
            _bettingRound.Blind(bb, _blinds.Big);
        }

        public void Check(Player player)
        {
            if (player == null) throw new NullReferenceException("Acting player can't be null");

            if (player.Id != _bettingRound.CurrentPlayer.Id) throw new InvalidOperationException("This player can't make actions.");

            _bettingRound.Fire(BettingTrigger.Check);
            _bettingRound.Fire(BettingTrigger.NextPlayer);
        }

        public void Call(Player player)
        {
            if (player == null) throw new NullReferenceException("Acting player can't be null");

            if (player.Id != _bettingRound.CurrentPlayer.Id) throw new InvalidOperationException("This player can't make actions.");

            _bettingRound.Fire(BettingTrigger.Call);
            _bettingRound.Fire(BettingTrigger.NextPlayer);
        }

        public void Raise(Player player, int bet)
        {
            if (player == null) throw new NullReferenceException("Acting player can't be null");

            if (player.Id != _bettingRound.CurrentPlayer.Id) throw new InvalidOperationException("This player can't make actions.");

            if (bet <= LastBet || bet < Blinds.Big) throw new InvalidOperationException("Bet size must be bigger then LastBet or current BigBlind");

            _bettingRound.Fire(BettingTrigger.Raise, bet);
            _bettingRound.Fire(BettingTrigger.NextPlayer);
        }

        public void AllIn(Player player)
        {
            if (player == null) throw new NullReferenceException("Acting player can't be null");

            if (player.Id != _bettingRound.CurrentPlayer.Id) throw new InvalidOperationException("This player can't make actions.");

            _bettingRound.Fire(BettingTrigger.AllIn);
            _bettingRound.Fire(BettingTrigger.NextPlayer);
        }

        public void Fold(Player player)
        {
            if (player == null) throw new NullReferenceException("Acting player can't be null");

            if (player.Id != _bettingRound.CurrentPlayer.Id) throw new InvalidOperationException("This player can't make actions.");

            _bettingRound.Fire(BettingTrigger.Fold);
            _bettingRound.Fire(BettingTrigger.NextPlayer);
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

            _bettingRound.Setup(_players.Where(x => x.BettingState != PlayerBettingState.Fold).ToList(), roundType);

            if (gameState == GameState.PreflopBetting)
                SetBlinds(_players.First(x => x.Position == PlayerPosition.SB), _players.First(x => x.Position == PlayerPosition.BB));
        }

        public Player GetCurrentPlayer()
        {
            return _bettingRound.CurrentPlayer;
        }

        private void ProcessAfterBettingRound()
        {
            TotalBank += _bettingRound.GetBankAmount();
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
                ProcessAfterBettingRound();
                _roundEnded = true;
            }
            if (eventArgs.PropertyName == nameof(_bettingRound.LastBet))
            {
                LastBet = _bettingRound.LastBet;
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
