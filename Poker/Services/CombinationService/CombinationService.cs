using Poker.Interfaces;
using Poker.Structs;
using System.Reflection;

namespace Poker.Services.CombinationService
{
    public class CombinationService : ICombinationCalculator
    {
        /// <summary>
        /// Returns combination from hand + desk cards
        /// </summary>
        /// <param name="hand"></param>
        /// <param name="desk"></param>
        /// <returns>CombinationResult</returns>
        /// <exception cref="ApplicationException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public CombinationResult GetCombination(List<Card> hand, List<Card> desk)
        {
            var allCards = hand.ToList();
            allCards.AddRange(desk);

            if (allCards.Count < 7) throw new ArgumentException("Need 7 cards to get combination");

            SortAsc(allCards);

            var methods = GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).Where(x => x.ReturnType == typeof(CheckMethodResponse));

            var res = new CheckMethodResponse(false, CombinationType.None, []);

            foreach (var method in methods)
            {
                res = method.Invoke(null, [allCards]) as CheckMethodResponse ?? throw new ApplicationException();

                if (res.HasCombination)
                {
                    if (res.Cards.Length == 5)
                    {
                        return new CombinationResult(res.Combination, [.. res.Cards ?? []]);
                    }

                    List<Card> combCards = [];

                    combCards.AddRange(res.Cards);

                    for (var i = 6; i > 0; i--)
                    {
                        var c = allCards.SkipLast(6 - i).Last();
                        if (!combCards.Contains(c))
                        {
                            combCards.Add(c);
                        }

                        if (combCards.Count == 5)
                        {
                            break;
                        }
                    }

                    SortAsc(combCards);

                    return new CombinationResult(res.Combination, [.. combCards ?? []]);
                }
            }

            return new CombinationResult(res.Combination, [.. res.Cards ?? []]);
        }

        private static (int start, int end, bool hasStraight) HasStraight(int[] cards)
        {
            cards = [.. cards.Distinct()];

            var straightStart = cards[0];
            var straightEnd = cards[0];
            var hasStraight = false;
            var count = 1;

            for (var i = 0; i < cards.Length - 1; i++)
            {
                var diff = cards[i + 1] - cards[i];

                if (diff > 1 && straightStart == straightEnd && !hasStraight)
                {
                    straightStart = cards[i + 1];
                    straightEnd = cards[i + 1];
                    count = 1;
                }

                if (diff == 1)
                {
                    count++;
                    straightEnd = cards[i + 1];
                    hasStraight = straightEnd - straightStart >= 4 && count >= 5;
                }
            }

            if (straightEnd - straightStart >= 4 && hasStraight)
            {
                return (straightEnd - 4, straightEnd, true);
            }

            return (0, 0, false);
        }

        private static CheckMethodResponse HasStraightOrFlush(List<Card> cards)
        {
            var hasStraight = HasStraight([.. cards.Select(x => (int)x.Rank)]);

            int[] suitesArr = new int[4];

            _ = cards.Select(x => suitesArr[(int)x.Suit]++).ToList();

            var hasFlush = suitesArr.Where(x => x >= 5).Any();

            if (hasStraight.hasStraight)
            {
                var straightCards = cards.Where(x => (int)x.Rank >= hasStraight.start && (int)x.Rank <= hasStraight.end).ToList();

                // Если карт 5 и масть одна, то либо стрит флеш либо флеш рояль
                if (straightCards.Select(x => x.Suit).Distinct().Count() == 1 && straightCards.Count == 5)
                {
                    if (straightCards.Last().Rank == Rank.Ace)
                    {
                        return new CheckMethodResponse(true, CombinationType.RoyalFlush, [.. straightCards]);
                    }

                    return new CheckMethodResponse(true, CombinationType.StraightFlush, [.. straightCards]);
                }
                // Иначе, если карт > 5, ищем, нет ли пяти карт с одинаковой мастью
                else if (straightCards.Count > 5)
                {
                    var suites = straightCards.Select(x => x.Suit).ToList();

                    foreach (var suite in suites)
                    {
                        var cardsToCheck = straightCards.Where(x => x.Suit == suite);

                        if (cardsToCheck.Count() == 5)
                        {
                            if (straightCards.Last().Rank == Rank.Ace)
                            {
                                return new CheckMethodResponse(true, CombinationType.RoyalFlush, [.. cardsToCheck]);
                            }

                            return new CheckMethodResponse(true, CombinationType.StraightFlush, [.. cardsToCheck]);
                        }
                    }
                }

                if (hasFlush)
                {
                    var flushSuit = (Suit)Array.IndexOf(suitesArr, suitesArr.Where(x => x >= 5).First());

                    return new CheckMethodResponse(true, CombinationType.Flush, [.. cards.Where(x => x.Suit == flushSuit).TakeLast(5)]);
                }

                return new CheckMethodResponse(true, CombinationType.Straight, [.. straightCards.DistinctBy(x => x.Rank)]);
            }

            if (hasFlush)
            {
                var flushSuit = (Suit)Array.IndexOf(suitesArr, suitesArr.Where(x => x >= 5).First());

                return new CheckMethodResponse(true, CombinationType.Flush, [.. cards.Where(x => x.Suit == flushSuit).TakeLast(5)]);
            }

            return new CheckMethodResponse(false, CombinationType.None, []);
        }

        private static CheckMethodResponse HasMultiple(List<Card> cards)
        {
            int[] arr = new int[(int)Rank.Ace + 1];

            for (var i = 0; i < cards.Count; i++)
            {
                arr[(int)cards[i].Rank]++;
            }

            if (arr.Any(x => x == 4))
            {
                var i = Array.IndexOf(arr, 4);

                return new CheckMethodResponse(true, CombinationType.Quad, [.. cards.Where(x => x.Rank == (Rank)i)]);
            }

            if (arr.Any(x => x == 3 && arr.Any(x => x == 2)) || Array.IndexOf(arr, 3) != Array.LastIndexOf(arr, 3))
            {
                var count = arr.Where(x => x == 3).Count();

                var i = 0;
                var j = 0;

                if (count == 1)
                {
                    i = Array.IndexOf(arr, 2);

                    j = Array.IndexOf(arr, 3);
                }

                i = Array.IndexOf(arr, 3);

                j = Array.LastIndexOf(arr, 3);

                return new CheckMethodResponse(true, CombinationType.FullHouse, [.. cards.Where(x => x.Rank == (Rank)i || x.Rank == (Rank)j).TakeLast(5)]);
            }

            if (arr.Any(x => x == 3))
            {
                var i = Array.IndexOf(arr, 3);

                return new CheckMethodResponse(true, CombinationType.Set, [.. cards.Where(x => x.Rank == (Rank)i)]);
            }

            if (arr.Any(x => x == 2))
            {
                var i = Array.IndexOf(arr, 2);

                if (arr.Count(x => x == 2) > 1)
                {
                    var j = Array.LastIndexOf(arr, 2);

                    return new CheckMethodResponse(true, CombinationType.TwoPairs, [.. cards.Where(x => x.Rank == (Rank)i || x.Rank == (Rank)j)]);
                }
                return new CheckMethodResponse(true, CombinationType.Pair, [.. cards.Where(x => x.Rank == (Rank)i)]);
            }


            return new CheckMethodResponse(true, CombinationType.Kicker, [cards.Last()]);
        }

        private static void SortAsc(List<Card> cards)
        {
            for (int i = 0; i < cards.Count - 1; i++)
            {
                for (int j = 0; j < cards.Count - 1; j++)
                {
                    if (cards[j].Rank > cards[j + 1].Rank)
                    {
                        (cards[j + 1], cards[j]) = (cards[j], cards[j + 1]);
                    }
                }
            }
        }

        public record CheckMethodResponse(bool HasCombination, CombinationType Combination, Card[] Cards);
    }
}
