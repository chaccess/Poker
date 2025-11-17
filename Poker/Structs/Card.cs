namespace Poker.Structs
{
    public struct Card(int r, int s) : IComparable<Card>
    {
        public Rank Rank { get; set; } = Enum.Parse<Rank>(r.ToString());

        public Suit Suit { get; set; } = Enum.Parse<Suit>(s.ToString());

        public readonly int CompareTo(Card obj)
        {
            return Rank.CompareTo(obj.Rank);
        }

        public static bool operator ==(Card left, Card right)
        {
            return left.Rank == right.Rank && left.Suit == right.Suit;
        }

        public static bool operator !=(Card left, Card right)
        {
            return !(left == right);
        }

        public override readonly bool Equals(object? obj)
        {
            if (obj is Card c)
                return this == c;
            return false;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Rank, Suit);
        }

        public override readonly string ToString()
        {
            return $"{Rank} {Suit}";
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
