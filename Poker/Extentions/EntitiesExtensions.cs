using Poker.Entities;
using Poker.Structs;
using System.Text;

namespace Poker.Extentions
{
    public static class EntitiesExtensions
    {
        public static string ToCustomString(this Card[] value)
        {

            if (value is null) return string.Empty;

            var sb = new StringBuilder();

            foreach (Card v in value)
            {
                sb.Append(v.Rank);
                sb.Append(' ');
                sb.Append(v.Suit);
                sb.Append(" || ");
            }
            sb.Remove(sb.Length - 4, 4);

            return sb.ToString();
        }

        public static string ToCustomString(this List<Card> value)
        {

            if (value is null) return string.Empty;

            var sb = new StringBuilder();

            foreach (Card v in value)
            {
                sb.Append(v.Rank);
                sb.Append(' ');
                sb.Append(v.Suit);
                sb.Append(" || ");
            }
            sb.Remove(sb.Length - 4, 4);

            return sb.ToString();
        }

        public static string GetNamesString(this List<Player> value)
        {

            if (value is null) return string.Empty;

            var sb = new StringBuilder();

            foreach (Player v in value)
            {
                sb.Append(v.Name);
                sb.Append(", ");
            }
            sb.Remove(sb.Length - 2, 2);

            return sb.ToString();
        }

        public static List<T> Clone<T>(this List<T> value)
        {
            return [.. value];
        }

        public static string ElementsToString<T>(this List<T> values)
        {
            var sb = new StringBuilder();

            foreach (T v in values)
            {
                sb.Append($"{v?.ToString()}, ");
            }
            sb.Remove(sb.Length - 2, 2);

            return sb.ToString();
        }
    }
}
