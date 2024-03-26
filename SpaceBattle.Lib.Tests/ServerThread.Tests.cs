using System;
using System.Collections.Concurrent;
using System.Threading;
using Hwdtech;
using Hwdtech.Ioc;
using Moq;
namespace SpaceBattle.Lib.Test;

public class ServerTheardTests
{
    public ServerTheardTests()
    {

        new InitScopeBasedIoCImplementationCommand().Execute();
        var scope = IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Root"));

        IoC.Resolve<Hwdtech.ICommand>("Scopes.Current.Set", scope).Execute();

        var idDict = new ConcurrentDictionary<int, ServerThread>();
        var qDict = new ConcurrentDictionary<int, BlockingCollection<ICommand>>();

        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Server.Dict", (object[] args) => idDict).Execute();
        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Server.QueueDict", (object[] args) => qDict).Execute();

        IoC.Resolve<Hwdtech.ICommand>("IoC.Register",
            "Create and Start Thread",
            (object[] args) =>
            {
                return new ActionCommand(() =>
                    {
                        var q = new BlockingCollection<ICommand>(10);
                        var st = new ServerThread(q, IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current")));
                        idDict.TryAdd((int)args[0], st);
                        qDict.TryAdd((int)args[0], q);
                        var thread = IoC.Resolve<ConcurrentDictionary<int, ServerThread>>("Server.Dict")[(int)args[0]];

                        st.Start();
                        if (args.Length == 2 && args[1] != null)
                        {
                            new ActionCommand((Action)args[1]).Execute();
                        }
                    }
                );
            }
        ).Execute();

        IoC.Resolve<Hwdtech.ICommand>("IoC.Register",
            "Add Command To QueueDict",
            (object[] args) =>
            {
                return new ActionCommand(() =>
                    {
                        qDict.TryAdd((int)args[0], (BlockingCollection<ICommand>)args[1]);
                    }
                );
            }
        ).Execute();

        IoC.Resolve<Hwdtech.ICommand>("IoC.Register",
            "Send Command",
            (object[] args) =>
            {
                return new ActionCommand(() =>
                    {
                        var qu = IoC.Resolve<ConcurrentDictionary<int, BlockingCollection<ICommand>>>("Server.QueueDict")[(int)args[0]];
                        qu.Add((ICommand)args[1]);
                        if (args.Length == 3 && args[2] != null)
                        {
                            new ActionCommand((Action)args[2]).Execute();
                        }
                    }
                );
            }
        ).Execute();

        IoC.Resolve<Hwdtech.ICommand>("IoC.Register",
            "Hard Stop The Thread",
            (object[] args) =>
            {
                return new ActionCommand(() =>
                    {
                        var thread = IoC.Resolve<ConcurrentDictionary<int, ServerThread>>("Server.Dict")[(int)args[0]];
                        new HardStop(thread).Execute();
                        if (args.Length == 2 && args[1] != null)
                        {
                            new ActionCommand((Action)args[1]).Execute();
                        }
                    }
                );
            }
        ).Execute();

        IoC.Resolve<Hwdtech.ICommand>("IoC.Register",
            "Soft Stop The Thread",
            (object[] args) =>
            {
                return new ActionCommand(() =>
                    {
                        var thread = IoC.Resolve<ConcurrentDictionary<int, ServerThread>>("Server.Dict")[(int)args[0]];
                        var q = IoC.Resolve<ConcurrentDictionary<int, BlockingCollection<ICommand>>>("Server.QueueDict")[(int)args[0]];
                        new SoftStop(thread, q, (Action)args[1]).Execute();
                    }
                );
            }
        ).Execute();
    }

    [Xunit.Fact]
    public void HardStopShouldStopServerThread()
    {
        IoC.Resolve<Hwdtech.ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current"))).Execute();

        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "ExceptionHandler.Handle", (object[] args) => new ActionCommand(() => { })).Execute();

        IoC.Resolve<ICommand>("Create and Start Thread", 1).Execute();

        var cmd = new Mock<ICommand>();
        cmd.Setup(m => m.Execute()).Verifiable();

        var mre = new ManualResetEvent(false);
        var hs = IoC.Resolve<ICommand>("Hard Stop The Thread", 1, () => { mre.Set(); });

        IoC.Resolve<ICommand>("Send Command", 1, cmd.Object).Execute();
        IoC.Resolve<ICommand>("Send Command", 1, hs).Execute();
        IoC.Resolve<ICommand>("Send Command", 1, cmd.Object).Execute();

        mre.WaitOne(1000);

        Xunit.Assert.Single(IoC.Resolve<ConcurrentDictionary<int, BlockingCollection<ICommand>>>("Server.QueueDict")[1]);
        cmd.Verify(m => m.Execute(), Times.Once);
    }

    [Xunit.Fact]
    public void HardStopShouldStopServerThreadWithCommandWithException()
    {
        IoC.Resolve<Hwdtech.ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current"))).Execute();

        var cmd = new Mock<ICommand>();

        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "ExceptionHandler.Handle", (object[] args) => cmd.Object).Execute();

        IoC.Resolve<ICommand>("Create and Start Thread", 2).Execute();

        var mre = new ManualResetEvent(false);
        var hs = IoC.Resolve<ICommand>("Hard Stop The Thread", 2, () => { mre.Set(); });

        var ecmd = new Mock<ICommand>();
        ecmd.Setup(m => m.Execute()).Throws(new Exception());

        IoC.Resolve<ICommand>("Send Command", 2, ecmd.Object).Execute();
        IoC.Resolve<ICommand>("Send Command", 2, hs).Execute();
        IoC.Resolve<ICommand>("Send Command", 2, ecmd.Object).Execute();

        mre.WaitOne(1000);

        Xunit.Assert.Throws<Exception>(() => hs.Execute());
        Xunit.Assert.Single(IoC.Resolve<ConcurrentDictionary<int, BlockingCollection<ICommand>>>("Server.QueueDict")[2]);
        cmd.Verify(m => m.Execute(), Times.Once);
    }

    [Xunit.Fact]
    public void HardStopCanNotStopServerBecauseOfWrongThread()
    {
        IoC.Resolve<Hwdtech.ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current"))).Execute();

        IoC.Resolve<ICommand>("Create and Start Thread", 5).Execute();

        var mre = new ManualResetEvent(false);

        var hs = IoC.Resolve<ICommand>("Hard Stop The Thread", 5, () => { mre.Set(); });

        IoC.Resolve<ICommand>("Send Command", 5, hs).Execute();

        mre.WaitOne(1000);

        Xunit.Assert.Throws<Exception>(() => hs.Execute());
        Xunit.Assert.Empty(IoC.Resolve<ConcurrentDictionary<int, BlockingCollection<ICommand>>>("Server.QueueDict")[5]);
    }

    [Xunit.Fact]
    public void SoftStopShouldStopServerThread()
    {
        IoC.Resolve<Hwdtech.ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current"))).Execute();

        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "ExceptionHandler.Handle", (object[] args) => new ActionCommand(() => { })).Execute();

        IoC.Resolve<ICommand>("Create and Start Thread", 3).Execute();

        var mre = new ManualResetEvent(false);

        var ss = IoC.Resolve<ICommand>("Soft Stop The Thread", 3, () => { mre.Set(); },
            IoC.Resolve<ConcurrentDictionary<int, BlockingCollection<ICommand>>>("Server.QueueDict")[3]);

        var cmd = new Mock<ICommand>();
        cmd.Setup(m => m.Execute()).Verifiable();

        IoC.Resolve<ICommand>("Send Command", 3, cmd.Object).Execute();
        IoC.Resolve<ICommand>("Send Command", 3, ss).Execute();
        IoC.Resolve<ICommand>("Send Command", 3, cmd.Object).Execute();
        IoC.Resolve<ICommand>("Send Command", 3, cmd.Object).Execute();

        mre.WaitOne(1000);

        Xunit.Assert.Empty(IoC.Resolve<ConcurrentDictionary<int, BlockingCollection<ICommand>>>("Server.QueueDict")[3]);
        cmd.Verify(m => m.Execute(), Times.AtLeast(2));
    }

    [Xunit.Fact]
    public void SoftStopShouldStopServerThreadWithCommandWithException()
    {
        IoC.Resolve<Hwdtech.ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current"))).Execute();

        var cmd = new Mock<ICommand>();

        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "ExceptionHandler.Handle", (object[] args) => cmd.Object).Execute();

        IoC.Resolve<ICommand>("Create and Start Thread", 4).Execute();

        var mre = new ManualResetEvent(false);

        var ss = IoC.Resolve<ICommand>("Soft Stop The Thread", 4, () => { mre.Set(); },
            IoC.Resolve<ConcurrentDictionary<int, BlockingCollection<ICommand>>>("Server.QueueDict")[4]);

        var ecmd = new Mock<ICommand>();
        ecmd.Setup(m => m.Execute()).Throws(new Exception());

        IoC.Resolve<ICommand>("Send Command", 4, ecmd.Object).Execute();
        IoC.Resolve<ICommand>("Send Command", 4, ss).Execute();
        IoC.Resolve<ICommand>("Send Command", 4, ecmd.Object).Execute();
        IoC.Resolve<ICommand>("Send Command", 4, ecmd.Object).Execute();

        mre.WaitOne(1000);

        Xunit.Assert.Throws<Exception>(() => ss.Execute());
        Xunit.Assert.Empty(IoC.Resolve<ConcurrentDictionary<int, BlockingCollection<ICommand>>>("Server.QueueDict")[4]);
    }

    [Xunit.Fact]
    public void HashCodeTheSame()
    {
        var q1 = new BlockingCollection<ICommand>();
        var sT1 = new ServerThread(q1, IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current")));
        var q2 = new BlockingCollection<ICommand>();
        var sT2 = new ServerThread(q2, IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current")));
        Xunit.Assert.True(sT1.GetHashCode() != sT2.GetHashCode());
    }

    [Xunit.Fact]
    public void EqualThreadsWithNull()
    {
        var q = new BlockingCollection<ICommand>(10);
        var st = new ServerThread(q, IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current")));
        Xunit.Assert.False(st.Equals(null));
    }

    [Xunit.Fact]
    public void PositiveEqualThreads()
    {
        var q1 = new BlockingCollection<ICommand>(10);

        var st1 = new ServerThread(q1, Thread.CurrentThread);
        var st2 = new ServerThread(q1, Thread.CurrentThread);

        Xunit.Assert.False(st1.Equals(st2));
    }

    [Xunit.Fact]
    public void AbsoluteDifferentEquals()
    {
        var q = new BlockingCollection<ICommand>(10);

        var st1 = new ServerThread(q, Thread.CurrentThread);
        var not_st = 15;

        Xunit.Assert.False(st1.Equals(not_st));
    }
}