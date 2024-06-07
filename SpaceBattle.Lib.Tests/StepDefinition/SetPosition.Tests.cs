using System.Collections.Generic;
using Hwdtech;
using Hwdtech.Ioc;
using Moq;
using Xunit;

namespace SpaceBattle.Lib.Tests;

public class SetPositionTest
{
    [Fact]
    public void SetPositioTest()
    {
        var mcmd = new Mock<ICommand>();
        mcmd.Setup(_m => _m.Execute()).Verifiable();

        var mStrat = new Mock<IStrategy>();
        mStrat.Setup(_m => _m.Run(It.IsAny<object[]>())).Returns(mcmd.Object);

        new InitScopeBasedIoCImplementationCommand().Execute();
        IoC.Resolve<Hwdtech.ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Root"))).Execute();

        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Game.SetIniPos", (object[] props) => new SetPositionStrategy().Run(props)).Execute();
        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Services.GetStartingPoint", (object[] props) => (object)new Vector(1, 1)).Execute();
        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Game.UObject.Set", (object[] props) => mStrat.Object.Run(props)).Execute();

        var poit = new PositionIterator(new List<int> { 3, 3 }, 2, 4);
        var iterStrat = new PositionIterWithMovement(poit);

        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Game.IniPosIter.Next", (object[] props) => iterStrat.Run()).Execute();

        Mock<IUObject> patient = new();

        IoC.Resolve<ICommand>("Game.SetIniPos", patient.Object).Execute();
        IoC.Resolve<ICommand>("Game.SetIniPos", patient.Object).Execute();
        IoC.Resolve<ICommand>("Game.SetIniPos", patient.Object).Execute();
        IoC.Resolve<ICommand>("Game.SetIniPos", patient.Object).Execute();
        IoC.Resolve<ICommand>("Game.SetIniPos", patient.Object).Execute();
        IoC.Resolve<ICommand>("Game.SetIniPos", patient.Object).Execute();

        mcmd.VerifyAll();

        poit.Dispose();
    }
}
