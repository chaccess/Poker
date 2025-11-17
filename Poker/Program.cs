using Poker.Entities;
using Poker.Structs;
using Poker.Services.BettingService;

var table = new Table(new Blinds(10, 20), TableStatus.Normal);

table.AddPlayer(new Player("Max"), 0);
table.AddPlayer(new Player("Nika"), 1);
table.AddPlayer(new Player("Sanya"), 3);
table.AddPlayer(new Player("Kostyan"), 4);
table.AddPlayer(new Player("Jenek"), 5);

table.Fire(GameStateTrigger.Init);
table.Fire(GameStateTrigger.StartDealHands);
table.Fire(GameStateTrigger.StartPreflopBetting);

// пока зашит прямой вариант(перепишу через while(BettingState != completed) для удобства)
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
table.Fire(GameStateTrigger.EndGame);