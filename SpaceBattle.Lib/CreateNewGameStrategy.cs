namespace SpaceBattle.Lib;
using Hwdtech;

public class CreateNewGame : IStrategyRenamed
{
    public object Strategy(params object[] args)
    {
        var gameId = (string)args[0];

        var commandQueue = new Queue<ICommand>();

        var gamesDictionary = IoC.Resolve<Dictionary<string, Queue<ICommand>>>("Get Dict Of Games");
        gamesDictionary.Add(gameId, commandQueue);

        return new GameCommand(gameId);
    }
}