using Poker.Entities;
using Poker.Structs;
using System.Runtime.CompilerServices;
using System.Text;

namespace Poker.Extentions
{
    public static class GenericExtentions
    {
        public static List<T> Clone<T>(this List<T> value)
        {
            return [.. value];
        }

        public static string ElementsToString<T>(this List<T> values)
        {
            if (values.Count == 0) return string.Empty;

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
