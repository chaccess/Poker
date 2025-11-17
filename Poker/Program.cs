using Poker.Entities;
using Poker.Structs;
using Poker.Extentions;
using Poker.Services.BettingMechanism;

var table = new Table(new Blinds(10, 20), TableStatus.Normal);

table.AddPlayer(new Player("Max"), 0);
table.AddPlayer(new Player("Nika"), 1);

table.Fire(GameStateTrigger.Init);
table.Fire(GameStateTrigger.StartDealHands);
table.Fire(GameStateTrigger.StartPreflopBetting);

string[] actions = ["C", "R", "A", "K", "F"];

MakeBettingRound();

table.Fire(GameStateTrigger.StartDealFlop);
table.Fire(GameStateTrigger.StartFlopBetting);

MakeBettingRound();

table.Fire(GameStateTrigger.SartDealTurn);
table.Fire(GameStateTrigger.StartTurnBetting);

MakeBettingRound();

table.Fire(GameStateTrigger.StartDealRiver);
table.Fire(GameStateTrigger.StartRiverBetting);

MakeBettingRound();

table.Fire(GameStateTrigger.StartShowDown);
table.Fire(GameStateTrigger.StartEvaluateHands);
table.Fire(GameStateTrigger.StartPayout);
table.Fire(GameStateTrigger.EndGame);

void RemovePlayer()
{
    //table.RemovePlayer(1);
}

void MakeBettingRound()
{
    if (!table.NeedBettingRound)
    {
        return;
    }
    Console.WriteLine();

    Task.Run(async () =>
    {
        await Task.Delay(3000);
        RemovePlayer();
    });

    while (!table.BettingRoundEnded)
    {
        try
        {
            var currentPlayer = table.GetCurrentPlayer();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine();
            Console.WriteLine("=================");
            Console.WriteLine($"Your cards: {currentPlayer.Hand.ElementsToString()}");
            Console.WriteLine("=================");
            Console.WriteLine();
            if (table.Desc.Count > 0)
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine("=================");
                Console.WriteLine($"Desk: {table.Desc.ElementsToString()}");
                Console.WriteLine("=================");
                Console.WriteLine();
            }
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"{currentPlayer.Name}, your turn");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("[NOTE] ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Type one letter to make an action - C/R/A/K/F for call/raise/allin/check/fold");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"Available actions: {currentPlayer.AvailableActions.ElementsToString()}:");

            var action = Console.ReadLine()?.ToUpper();

            if (actions.Contains(action))
            {
                if (action == "C")
                    table.MakePlayerAction(currentPlayer.Id, BettingTrigger.Call);
                if (action == "K")
                    table.MakePlayerAction(currentPlayer.Id, BettingTrigger.Check);
                if (action == "A")
                    table.MakePlayerAction(currentPlayer.Id, BettingTrigger.AllIn);
                if (action == "F")
                    table.MakePlayerAction(currentPlayer.Id, BettingTrigger.Fold);
                if (action == "R")
                {
                    Console.Write($"Your bet(must be bigger than {table.LastBet}): ");
                    var betStr = Console.ReadLine();
                    var ok = int.TryParse(betStr, out int bet);

                    if (ok)
                        table.MakePlayerAction(currentPlayer.Id, BettingTrigger.Raise, bet);
                }
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(ex.Message.ToString());
            Console.ForegroundColor = ConsoleColor.White;
            continue;
        }
    }
}