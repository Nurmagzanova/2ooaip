using System.Collections.Concurrent;
using Hwdtech;

namespace SpaceBattle.Lib;

public class SoftStop : ICommand
{
    private readonly BlockingCollection<ICommand> _queue;
    private readonly ServerThread _thread;
    public Action action = () => { };
    public SoftStop(ServerThread thread, BlockingCollection<ICommand> queue, Action action)
    {
        _thread = thread;
        _queue = queue;
        this.action = action;
    }

    public void Execute()
    {
        if (_thread.Equals(Thread.CurrentThread))
        {
            _thread.UpdateBehaviour(() =>
            {
                if (_queue.TryTake(out var command) == true)
                {
                    var cmd = _queue.Take();
                    try
                    {
                        cmd.Execute();
                    }
                    catch (Exception e)
                    {
                        IoC.Resolve<ICommand>("ExceptionHandler.Handle", cmd, e).Execute();
                    }
                }
                else
                {
                    action();
                    _thread.Stop();
                }
            });
        }
        else
        {
            throw new Exception("Wrong Thread");
        }
    }
}
