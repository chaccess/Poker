using Poker.Entities;
using Poker.Extentions;
using Poker.Services.CombinationService;
using Poker.Structs;
using Poker.Services.BettingService;

var table = new Table(new Blinds(10, 20), TableStatus.Normal);

table.AddPlayer(new Player("Max"), 0);
table.AddPlayer(new Player("Nika"), 1);
table.AddPlayer(new Player("Maggy"), 3);
table.AddPlayer(new Player("Olga"), 4);
table.AddPlayer(new Player("Alexey"), 5);

table.Fire(GameStateTrigger.Init);
table.Fire(GameStateTrigger.StartDealHands);
table.Fire(GameStateTrigger.StartPreflopBetting);

var currentPlayer = table.GetCurrentPlayer();
table.MakePlayerAction(currentPlayer.Id, BettingTrigger.Call);
currentPlayer = table.GetCurrentPlayer();
table.MakePlayerAction(currentPlayer.Id, BettingTrigger.Raise, table.LastBet + 20);
currentPlayer = table.GetCurrentPlayer();
table.MakePlayerAction(currentPlayer.Id, BettingTrigger.Call);
currentPlayer = table.GetCurrentPlayer();
table.MakePlayerAction(currentPlayer.Id, BettingTrigger.Call);
currentPlayer = table.GetCurrentPlayer();
table.MakePlayerAction(currentPlayer.Id, BettingTrigger.Fold);
currentPlayer = table.GetCurrentPlayer();
table.MakePlayerAction(currentPlayer.Id, BettingTrigger.Call);

table.Fire(GameStateTrigger.StartDealFlop);
table.Fire(GameStateTrigger.StartFlopBetting);

currentPlayer = table.GetCurrentPlayer();
table.MakePlayerAction(currentPlayer.Id, BettingTrigger.Check);
currentPlayer = table.GetCurrentPlayer();
table.MakePlayerAction(currentPlayer.Id, BettingTrigger.Check);
currentPlayer = table.GetCurrentPlayer();
table.MakePlayerAction(currentPlayer.Id, BettingTrigger.Check);
currentPlayer = table.GetCurrentPlayer();
table.MakePlayerAction(currentPlayer.Id, BettingTrigger.Check);

table.Fire(GameStateTrigger.SartDealTurn);
table.Fire(GameStateTrigger.StartTurnBetting);

currentPlayer = table.GetCurrentPlayer();
table.MakePlayerAction(currentPlayer.Id, BettingTrigger.Raise, table.LastBet + 50);
currentPlayer = table.GetCurrentPlayer();
table.MakePlayerAction(currentPlayer.Id, BettingTrigger.Fold);
currentPlayer = table.GetCurrentPlayer();
table.MakePlayerAction(currentPlayer.Id, BettingTrigger.Call);
currentPlayer = table.GetCurrentPlayer();
table.MakePlayerAction(currentPlayer.Id, BettingTrigger.Call);

table.Fire(GameStateTrigger.StartDealRiver);
table.Fire(GameStateTrigger.StartRiverBetting);
table.Fire(GameStateTrigger.StartShowDown);
table.Fire(GameStateTrigger.StartEvaluateHands);
table.Fire(GameStateTrigger.StartPayout);



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