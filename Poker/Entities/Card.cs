namespace Poker.Entities
{
    public class Card(int r, int s) : IComparable
    {
        public Rank Rank { get; set; } = Enum.Parse<Rank>(r.ToString());

        public Suit Suit { get; set; } = Enum.Parse<Suit>(s.ToString());

        public int CompareTo(object? obj)
        {
            if (obj == null) return 1;

            if (obj == this) return 0;

            if (obj.GetType() == typeof(Card))
            {
                return ((Card)obj).Rank > Rank ? -1 : 1;
            }

            return 0;
        }
    }

    public enum Rank
    {
        Two = 2,
        Three = 3,
        Four = 4,
        Five = 5,
        Six = 6,
        Seven = 7,
        Eight = 8,
        Nine = 9,
        Ten = 10,
        Jack = 11,
        Queen = 12,
        King = 13,
        Ace = 14,
    }

    public enum Suit
    {
        Spades,
        Hearts,
        Dimonds,
        Clubs
    }
}
