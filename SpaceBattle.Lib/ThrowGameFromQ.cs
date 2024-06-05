namespace SpaceBattle.Lib;

public class ThrowFromQueue : IStrategyRenamed
{
    public object Strategy(params object[] param)
    {
        var commandQueue = (Queue<ICommand>)param[0];
        return commandQueue.Dequeue();
    }
}
