using Poker.Entities;
using Poker.Services.CombinationService;
using Poker.Structs;

namespace Tests;

public class TableTests
{
    private Croupier _croupier = null!;

    [SetUp]
    public void Setup()
    {
        _croupier = new Croupier("John");
    }

    [TestCase]
    public void TestTableAddPlayer()
    {
        var table = new Table(new Blinds(10, 20), TableStatus.Normal);
        var player = new Player("Max");

        table.AddPlayer(player, 1);

        Assert.That(table.Players.Any(x => x == player));
    }

    [TestCase]
    public void TestTableRemovePlayer()
    {
        var table = new Table(new Blinds(10, 20), TableStatus.Normal);
        var player = new Player("Max");

        table.AddPlayer(player, 1);
        table.RemovePlayer(1);

        Assert.That(table.Players, Is.Empty);

        table.AddPlayer(player, 1);
        table.RemovePlayer(2);

        Assert.That(table.Players.Any(x => x == player));
    }

    private static IEnumerable<TestCaseData> GetAddPlayerTestDate()
    {
        yield return new TestCaseData();
    }
}
