namespace SpaceBattle.Lib;

public class GamePushToQueue : ICommand
{
    public Queue<ICommand> commandQueue;
    public ICommand command;
    public GamePushToQueue(Queue<ICommand> commandQueue, ICommand command)
    {
        this.commandQueue = commandQueue;
        this.command = command;
    }
    public void Execute()
    {
        commandQueue.Enqueue(command);
    }
}