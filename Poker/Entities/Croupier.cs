using Poker.Interfaces;
using Poker.Structs;

namespace Poker.Entities
{
    public class Croupier : BaseEntity
    {
        public Croupier(string name)
        {
            Id = Guid.NewGuid();
            Name = name;
            GetNewDeck();
            ShuffleDeck();
        }

        public string Name { get; set; }

        public Stack<Card> Deck { get; set; } = new Stack<Card>();

        public List<Player> GetWinner(List<Player> players, List<Card> desc)
        {
            ArgumentNullException.ThrowIfNull(desc);
            var maxType = players.Max(x => x.CombinationResult.CombinationType);

            var condidates = players.Where(x => x.CombinationResult.CombinationType == maxType).ToList();

            if (condidates.Count == 1)
                return condidates;

            var combinations = condidates.Select(x => x.CombinationResult.CombinationCards).ToList();

            var kicker = combinations.Max(GetKicker);

            return [.. condidates.Where(x => x.CombinationResult.CombinationCards.Where(c => c.Rank == kicker).Any())];
        }

        public void Reset()
        {
            GetNewDeck();
            ShuffleDeck();
        }

        private void GetNewDeck()
        {
            Deck.Clear();
            for (int i = 2; i <= 14; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    Deck.Push(new Card(i, j));
                }
            }
        }

        private void ShuffleDeck()
        {
            var shuffledDeck = new Stack<Card>();
            var tmp = Deck.ToList();

            var r = new Random();

            while (tmp.Count > 0)
            {
                var rnd = r.Next(tmp.Count - 1);
                shuffledDeck.Push(tmp[rnd]);
                tmp.RemoveAt(rnd);
            }

            Deck = shuffledDeck;
        }

        public void DealStartHands(List<Player> players)
        {
            foreach (var player in players)
            {
                player.Hand.Add(Deck.Pop());
            }

            foreach (var player in players)
            {
                player.Hand.Add(Deck.Pop());
            }
        }

        public List<Card> DealFlop()
        {
            var cards = new List<Card>();

            Deck.Pop();

            for (int i = 0; i < 3; i++)
            {
                cards.Add(Deck.Pop());
            }

            return cards;
        }

        public Card DealTurn()
        {
            Deck.Pop();
            return Deck.Pop();
        }

        public Card DealRiver()
        {
            return DealTurn();
        }

        private Rank GetKicker(List<Card> cards)
        {
            return cards.Max(x => x.Rank);
        }
    }
}
