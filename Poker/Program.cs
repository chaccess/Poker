using Poker.Entities;
using Poker.Extentions;
using Poker.Services;
using Poker.Services.CombinationService;

var croupier = new Croupier("John");

var calculator = new CombinationService();


var desk = croupier.DealFlop();
desk.Add(croupier.DealTurn());
desk.Add(croupier.DealRiver());

var playersList = new List<Player>();

playersList.Add(new Player("Max"));
playersList.Add(new Player("Nika"));
playersList.Add(new Player("Maggy"));
playersList.Add(new Player("Olga"));
playersList.Add(new Player("Alexey"));

croupier.DealStartHands(playersList);

foreach (var player in playersList)
{
    Console.WriteLine($"{player.Name}: ");
    foreach (var card in player.Hand)
    {
        Console.WriteLine($"{card.Rank} {card.Suit}");
    }
    Console.WriteLine();
}

Console.WriteLine();
Console.WriteLine($"Стол:");
foreach (var card in desk)
{
    Console.WriteLine($"{card.Rank} {card.Suit}");
}

Console.WriteLine();
Console.WriteLine();

foreach (var player in playersList)
{
    player.CombinationResult = calculator.GetCombination(player.Hand, desk);
    Console.WriteLine();
    Console.WriteLine($"{player.Name} - {player.CombinationResult.CombinationType}: {player.CombinationResult.CombinationCards?.ToCustomString()}");
    Console.WriteLine();
}

var winners = croupier.GetWinner(playersList, desk);

Console.WriteLine();
Console.WriteLine();

if (winners.Count > 1)
{
    Console.WriteLine($"{winners.GetNamesString()} разделили банк с комбинацией {winners.First().CombinationResult.CombinationType}");
}
else
{
    Console.WriteLine($"{winners.First().Name} победил с комбинацией {winners.First().CombinationResult.CombinationType}");
}