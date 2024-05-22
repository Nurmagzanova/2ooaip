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

   