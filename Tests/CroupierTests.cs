using Poker.Entities;
using Poker.Services.CombinationService;
using Poker.Structs;

namespace Tests;

public class CroupierTests
{
    private Croupier _croupier = null!;

    [SetUp]
    public void Setup()
    {
        _croupier = new Croupier("John");
    }

    [TestCaseSource(nameof(GetWinnerTestData))]
    public void TestCroupierGetWinner(List<Player> players, List<Card> desc, List<Player> expectedWinners)
    {
        var winner = _croupier.GetWinner(players, desc);
        Assert.That(winner, Is.EqualTo(expectedWinners));
    }

    private static IEnumerable<TestCaseData> GetWinnerTestData()
    {
        // Пара тузов против пары королей → побеждает Макс
        var player1 = new Player("Max")
        {
            Hand = [new(14, 0), new(14, 1)],
            CombinationResult = new CombinationResult(CombinationType.Pair, [new(14, 0), new(14, 1)])
        };

        var player2 = new Player("Nika")
        {
            Hand = [new(13, 0), new(13, 1)],
            CombinationResult = new CombinationResult(CombinationType.Pair, [new(13, 0), new(13, 1)])
        };

        yield return new TestCaseData(
            new List<Player> { player1, player2 },
            new List<Card> { new(2, 2), new(5, 3), new(7, 1), new(10, 2), new(12, 0) },
            new List<Player> { player1 }
        ).SetName("Pair_Aces_vs_Pair_Kings");

        // Стрит против сета → побеждает стрит
        var player3 = new Player("Liam")
        {
            Hand = [new(9, 2), new(10, 3)],
            CombinationResult = new CombinationResult(CombinationType.Straight, [new(6, 0), new(7, 1), new(8, 2), new(9, 2), new(10, 3)])
        };

        var player4 = new Player("Emma")
        {
            Hand = [new(6, 2), new(6, 3)],
            CombinationResult = new CombinationResult(CombinationType.Set, [new(6, 0), new(6, 2), new(6, 3)])
        };

        yield return new TestCaseData(
            new List<Player> { player3, player4 },
            new List<Card> { new(6, 0), new(7, 1), new(8, 2), new(2, 3), new(11, 0) },
            new List<Player> { player3 }
        ).SetName("Straight_beats_ThreeOfKind");

        // Флеш против стрита → побеждает флеш
        var player5 = new Player("Noa")
        {
            Hand = [new(2, 3), new(9, 3)],
            CombinationResult = new CombinationResult(CombinationType.Flush, [new(2, 3), new(5, 3), new(7, 3), new(9, 3), new(11, 3)])
        };

        var player6 = new Player("Eli")
        {
            Hand = [new(5, 1), new(6, 2)],
            CombinationResult = new CombinationResult(CombinationType.Straight, [new(5, 1), new(6, 2), new(7, 0), new(8, 3), new(9, 2)])
        };

        yield return new TestCaseData(
            new List<Player> { player5, player6 },
            new List<Card> { new(5, 3), new(7, 3), new(11, 3), new(8, 2), new(10, 1) },
            new List<Player> { player5 }
        ).SetName("Flush_beats_Straight");

        // Два фулл-хауса одинаковой силы → ничья
        var player7 = new Player("Olga")
        {
            Hand = [new(10, 0), new(10, 1)],
            CombinationResult = new CombinationResult(CombinationType.FullHouse, [new(10, 0), new(10, 1), new(7, 0), new(7, 1), new(7, 2)])
        };

        var player8 = new Player("Dan")
        {
            Hand = [new(10, 2), new(7, 3)],
            CombinationResult = new CombinationResult(CombinationType.FullHouse, [new(10, 0), new(10, 2), new(7, 0), new(7, 1), new(7, 3)])
        };

        yield return new TestCaseData(
            new List<Player> { player7, player8 },
            new List<Card> { new(7, 0), new(7, 1), new(7, 2), new(10, 0), new(10, 1) },
            new List<Player> { player7, player8 }
        ).SetName("FullHouse_Tie");

        // Каре против фулл-хауса → побеждает каре
        var player9 = new Player("Kate")
        {
            Hand = [new(9, 0), new(9, 1)],
            CombinationResult = new CombinationResult(CombinationType.Quad, [new(9, 0), new(9, 1), new(9, 2), new(9, 3)])
        };

        var player10 = new Player("Leo")
        {
            Hand = [new(8, 0), new(8, 1)],
            CombinationResult = new CombinationResult(CombinationType.FullHouse, [new(8, 0), new(8, 1), new(8, 2), new(5, 3), new(5, 0)])
        };

        yield return new TestCaseData(
            new List<Player> { player9, player10 },
            new List<Card> { new(9, 2), new(9, 3), new(8, 2), new(5, 3), new(5, 0) },
            new List<Player> { player9 }
        ).SetName("FourOfKind_beats_FullHouse");

        // Роял-флеш против стрит-флеша → побеждает роял-флеш
        var player11 = new Player("Anna")
        {
            Hand = [new(14, 0), new(13, 0)],
            CombinationResult = new CombinationResult(CombinationType.RoyalFlush, [new(14, 0), new(13, 0), new(12, 0), new(11, 0), new(10, 0)])
        };

        var player12 = new Player("Tom")
        {
            Hand = [new(9, 1), new(8, 1)],
            CombinationResult = new CombinationResult(CombinationType.StraightFlush, [new(9, 1), new(8, 1), new(7, 1), new(6, 1), new(5, 1)])
        };

        yield return new TestCaseData(
            new List<Player> { player11, player12 },
            new List<Card> { new(12, 0), new(11, 0), new(10, 0), new(7, 1), new(6, 1) },
            new List<Player> { player11 }
        ).SetName("RoyalFlush_beats_StraightFlush");
    }
}
