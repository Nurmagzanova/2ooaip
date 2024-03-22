namespace SpaceBattle.Lib;

public class HardStop : ICommand
{
    private readonly ServerThread _thread;
    public HardStop(ServerThread thread)
    {
        _thread = thread;
    }

    public void Execute()
    {
        if (_thread.Equals(Thread.CurrentThread))
        {
            _thread.Stop();
        }
        else
        {
            throw new Exception("Wrong Thread");
        }
    }
}
