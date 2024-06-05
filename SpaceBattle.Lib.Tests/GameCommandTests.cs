namespace SpaceBattle.Lib.Test;

using System;
using System.Collections.Generic;
using Hwdtech;
using Hwdtech.Ioc;
using Moq;
using SpaceBattle.Lib;
using Xunit;

public class ActionCommand : SpaceBattle.Lib.ICommand
{
    private readonly Action _action;
    public ActionCommand(Action action)
    {
        _action = action;
    }

    public void Execute()
    {
        _action();
    }
}
public class GameTest
{
    public readonly Dictionary<string, object> scopeMap = new Dictionary<string, object>() {
        {"TheFirst", 1},
        {"TheSecond", 2},
        {"TheThird", 3}
    };
    public Dictionary<string, IReceiver> dictReceivers = new();
    public Dictionary<string, TimeSpan> dictTimes = new();

    public GameTest()
    {
        new InitScopeBasedIoCImplementationCommand().Execute();
        var scope = IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Root"));
        IoC.Resolve<Hwdtech.ICommand>("Scopes.Current.Set", scope).Execute();

        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Get Dict Of Games", (object[] args) =>
        {
            return new Dictionary<string, Queue<SpaceBattle.Lib.ICommand>>();
        }).Execute();

        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "ScopeMap", (object[] args) =>
        {
            return scopeMap;
        }).Execute();

        var getTimeStrategy = new Mock<IStrategyRenamed>();
        getTimeStrategy.Setup(s => s.Strategy(It.IsAny<object[]>())).Returns((object[] args) => dictTimes[(string)args[0]]);
        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "GetTime", (object[] args) => getTimeStrategy.Object.Strategy(args)).Execute();

        var getReceiverStrategy = new Mock<IStrategyRenamed>();
        getReceiverStrategy.Setup(s => s.Strategy(It.IsAny<object[]>())).Returns((object[] args) => dictReceivers[(string)args[0]]);
        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "GetReceiver", (object[] args) => getReceiverStrategy.Object.Strategy(args)).Execute();

        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "ExceptionHandler", (object[] args) =>
        {
            return new ActionCommand(() => { });
        }).Execute();

        var getScopeStrategy = new Mock<IStrategyRenamed>();
        getScopeStrategy.Setup(s => s.Strategy(It.IsAny<object[]>())).Returns((object[] args) => scope);
        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "GetScope", (object[] args) => getScopeStrategy.Object.Strategy(args)).Execute();
    }
    [Fact]
    public void Test_DeleteGame()
    {
        var gameId = "TheSecond";
        var deleteCommand = new Delete(gameId);
        deleteCommand.Execute();

        var scopeMap = IoC.Resolve<Dictionary<string, object>>("ScopeMap");
        Assert.False(scopeMap.ContainsKey(gameId));

    }
    [Fact]
    public void Test_CreateGame()
    {
        var gameId = "1";

        var createGameStrategy = new CreateNewGame();
        var result = createGameStrategy.Strategy(gameId);

        Assert.NotNull(result);
        Assert.IsType<GameCommand>(result);
    }
    [Fact]
    public void Test_GameObjectsDeleteGet()
    {
        var gameItemId = "1";
        var obj = new object();
        var objects = new Dictionary<string, object>()
        {
            { gameItemId, obj }
        };

        var result = new GetObject().Strategy(objects, gameItemId);
        Assert.Equal(obj, result);

        new DeleteObject(objects, gameItemId).Execute();
        Assert.DoesNotContain(gameItemId, objects.Keys);
    }
    [Fact]
    public void Test_GameQueuePushAndPop()
    {
        var commandMock = new Mock<SpaceBattle.Lib.ICommand>();
        var commandQueueMock = new Mock<Queue<SpaceBattle.Lib.ICommand>>();

        new GamePushToQueue(commandQueueMock.Object, commandMock.Object).Execute();
        Assert.True(commandQueueMock.Object.Contains(commandMock.Object));

        var command = new ThrowFromQueue().Strategy(commandQueueMock.Object);
        Assert.Equal(command, commandMock.Object);
    }
    [Fact]
    public void GameCommandTest()
    {
        var cmd1 = new Mock<SpaceBattle.Lib.ICommand>();
        cmd1.Setup(c => c.Execute()).Verifiable();
        var cmd2 = new Mock<SpaceBattle.Lib.ICommand>();
        cmd2.Setup(c => c.Execute()).Verifiable();
        var cmd3 = new Mock<SpaceBattle.Lib.ICommand>();
        cmd3.Setup(c => c.Execute()).Verifiable();

        var listCmds = new List<SpaceBattle.Lib.ICommand>() { cmd1.Object, cmd2.Object, cmd3.Object };

        dictReceivers.Add("1", new QueueAdapter(new Queue<SpaceBattle.Lib.ICommand>(listCmds)));
        dictTimes.Add("1", TimeSpan.FromSeconds(3));

        var game = new GameCommand("1");
        game.Execute();

        cmd1.Verify(c => c.Execute(), Times.Once());
        cmd2.Verify(c => c.Execute(), Times.Once());
        cmd3.Verify(c => c.Execute(), Times.Once());
    }

    [Fact]
    public void GameCommandExceptionTest()
    {
        var cmd = new Mock<SpaceBattle.Lib.ICommand>();
        cmd.Setup(c => c.Execute()).Throws<Exception>().Verifiable();
        var listCmds = new List<SpaceBattle.Lib.ICommand>() { cmd.Object };

        dictReceivers.Add("2", new QueueAdapter(new Queue<SpaceBattle.Lib.ICommand>(listCmds)));
        dictTimes.Add("2", TimeSpan.FromSeconds(3));

        var game = new GameCommand("2");
        game.Execute();

        cmd.Verify();
    }

    [Fact]
    public void ExceptionHandlerTest()
    {
        var count = 0;
        var exc = new Exception("101");
        var cmd = new Mock<SpaceBattle.Lib.ICommand>();
        var unused = cmd.Setup(c => c.Execute()).Throws(() => exc);

        var str = new Mock<IStrategyRenamed>();
        str.Setup(s => s.Strategy()).Callback(() => count++);

        IoC.Resolve<SpaceBattle.Lib.ICommand>("ExceptionHandler", cmd.Object, exc).Execute();

        cmd.Verify();
    }
}
