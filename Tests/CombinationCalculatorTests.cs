using Poker.Entities;
using Poker.Interfaces;
using Poker.Services.CombinationCalculator;

namespace Tests
{
    [TestFixture]
    public class CombinationCalculatorTests
    {
        private CombinationCalculator _calculator = null!;

        [SetUp]
        public void Setup()
        {
            _calculator = new CombinationCalculator();
        }

        [TestCaseSource(nameof(GetCombinationTestData))]
        public void TestCombinationCalculatorGetCombination(List<Card> hand, List<Card> desk, CombinationType expectedType, List<Card> expectedCards)
        {
            var result = _calculator.GetCombination(hand, desk);

            Assert.That(result.CombinationType, Is.EqualTo(expectedType), "Неверный тип комбинации");

            // Проверяем, что карты совпадают (по достоинству и масти)
            var actual = result.CombinationCards
                .OrderBy(c => (int)c.Rank).ThenBy(c => (int)c.Suit)
                .Select(c => ((int)c.Rank, (int)c.Suit))
                .ToList();

            var expected = expectedCards
                .OrderBy(c => (int)c.Rank).ThenBy(c => (int)c.Suit)
                .Select(c => ((int)c.Rank, (int)c.Suit))
                .ToList();

            Assert.That(actual, Is.EqualTo(expected), "Неверный набор карт в комбинации");
        }

        private static IEnumerable<TestCaseData> GetCombinationTestData()
        {
            // 1. Старшая карта (High Card)
            yield return new TestCaseData(
                new List<Card> { new(14, 0), new(9, 1) },
                new List<Card> { new(2, 2), new(5, 3), new(7, 1), new(10, 2), new(12, 0) },
                CombinationType.Kicker,
                new List<Card> { new(14, 0), new(12, 0), new(10, 2), new(9, 1), new(7, 1) }
            ).SetName("High_Card");

            // 2. Пара
            yield return new TestCaseData(
                new List<Card> { new(8, 0), new(13, 2) },
                new List<Card> { new(8, 2), new(5, 3), new(11, 1), new(3, 0), new(14, 1) },
                CombinationType.Pair,
                new List<Card> { new(8, 0), new(8, 2), new(11, 1), new(13, 2), new(14, 1) }
            ).SetName("One_Pair");

            // 3. Две пары
            yield return new TestCaseData(
                new List<Card> { new(14, 0), new(7, 2) },
                new List<Card> { new(14, 2), new(7, 0), new(10, 1), new(9, 3), new(4, 0) },
                CombinationType.TwoPairs,
                new List<Card> { new(14, 0), new(14, 2), new(7, 2), new(7, 0), new(10, 1) }
            ).SetName("Two_Pairs");

            // 4. Сет
            yield return new TestCaseData(
                new List<Card> { new(12, 0), new(3, 1) },
                new List<Card> { new(12, 1), new(12, 2), new(6, 3), new(8, 1), new(10, 0) },
                CombinationType.Set,
                new List<Card> { new(12, 0), new(12, 1), new(12, 2), new(8, 1), new(10, 0) }
            ).SetName("Three_of_a_Kind");

            // 5. Стрит
            yield return new TestCaseData(
                new List<Card> { new(5, 0), new(6, 1) },
                new List<Card> { new(7, 2), new(8, 3), new(9, 0), new(2, 2), new(13, 1) },
                CombinationType.Straight,
                new List<Card> { new(5, 0), new(6, 1), new(7, 2), new(8, 3), new(9, 0) }
            ).SetName("Straight");

            // 6. Флеш
            yield return new TestCaseData(
                new List<Card> { new(2, 1), new(10, 1) },
                new List<Card> { new(5, 1), new(7, 1), new(12, 1), new(8, 1), new(3, 2) },
                CombinationType.Flush,
                new List<Card> { new(12, 1), new(5, 1), new(7, 1), new(8, 1), new(10, 1) }
            ).SetName("Flush");

            // 7. Фулл-хаус
            yield return new TestCaseData(
                new List<Card> { new(10, 0), new(10, 1) },
                new List<Card> { new(10, 2), new(7, 0), new(7, 3), new(4, 1), new(3, 2) },
                CombinationType.FullHouse,
                new List<Card> { new(10, 0), new(10, 1), new(10, 2), new(7, 0), new(7, 3) }
            ).SetName("Full_House");

            // 8. Каре
            yield return new TestCaseData(
                new List<Card> { new(9, 0), new(9, 1) },
                new List<Card> { new(9, 2), new(9, 3), new(12, 1), new(7, 2), new(3, 0) },
                CombinationType.Quad,
                new List<Card> { new(9, 0), new(9, 1), new(9, 2), new(9, 3), new(12, 1) }
            ).SetName("Four_of_a_Kind");

            // 9. Стрит-флеш
            yield return new TestCaseData(
                new List<Card> { new(5, 0), new(6, 0) },
                new List<Card> { new(7, 0), new(8, 0), new(9, 0), new(2, 1), new(13, 2) },
                CombinationType.StraightFlush,
                new List<Card> { new(5, 0), new(6, 0), new(7, 0), new(8, 0), new(9, 0) }
            ).SetName("Straight_Flush");

            // 10. Роял-флеш
            yield return new TestCaseData(
                new List<Card> { new(14, 0), new(13, 0) },
                new List<Card> { new(12, 0), new(11, 0), new(10, 0), new(5, 1), new(7, 3) },
                CombinationType.RoyalFlush,
                new List<Card> { new(10, 0), new(11, 0), new(12, 0), new(13, 0), new(14, 0) }
            ).SetName("Royal_Flush");

            // 11. Конфликт комбинаций — флеш и фулл-хаус (выигрывает фулл-хаус)
            yield return new TestCaseData(
                new List<Card> { new(7, 1), new(7, 2) },
                new List<Card> { new(7, 3), new(10, 1), new(10, 2), new(10, 3), new(2, 1) },
                CombinationType.FullHouse,
                new List<Card> { new(7, 3), new(7, 2), new(10, 3), new(10, 1), new(10, 2) }
            ).SetName("Full_House_beats_Flush");

            // 12. Конфликт комбинаций — стрит и сет (выигрывает стрит)
            yield return new TestCaseData(
                new List<Card> { new(6, 1), new(7, 2) },
                new List<Card> { new(8, 3), new(9, 0), new(10, 2), new(6, 2), new(6, 3) },
                CombinationType.Straight,
                new List<Card> { new(6, 1), new(7, 2), new(8, 3), new(9, 0), new(10, 2) }
            ).SetName("Straight_beats_ThreeOfKind");

            // 13. Конфликт комбинаций — флеш и стрит (выигрывает флеш)
            yield return new TestCaseData(
                new List<Card> { new(2, 3), new(9, 3) },
                new List<Card> { new(5, 3), new(7, 3), new(11, 3), new(8, 2), new(10, 1) },
                CombinationType.Flush,
                new List<Card> { new(2, 3), new(5, 3), new(7, 3), new(9, 3), new(11, 3) }
            ).SetName("Flush_beats_Straight");

            // 14. Конфликт комбинаций — каре и фулл-хаус (каре старше)
            yield return new TestCaseData(
                new List<Card> { new(9, 0), new(9, 1) },
                new List<Card> { new(9, 2), new(9, 3), new(8, 2), new(8, 3), new(7, 1) },
                CombinationType.Quad,
                new List<Card> { new(9, 0), new(9, 1), new(9, 2), new(9, 3), new(8, 3) }
            ).SetName("FourOfKind_beats_FullHouse");
        }
    }
}
