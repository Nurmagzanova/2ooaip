using System.Collections.Generic;
using System.Linq;
using Hwdtech;
using Hwdtech.Ioc;
using Moq;
using Xunit;

namespace SpaceBattle.Lib.Tests;

public class CreateShipsCMDTests
{
    public CreateShipsCMDTests()
    {
        Mock<IUObject> uobj = new();

        Mock<IStrategy> createShipSt = new();
        createShipSt.Setup(_c => _c.Run()).Returns(uobj.Object);

        new InitScopeBasedIoCImplementationCommand().Execute();
        IoC.Resolve<Hwdtech.ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Root"))).Execute();

        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Game.Ship.Create", (object[] props) => createShipSt.Object.Run()).Execute();
        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Game.NumOfAllShips", (object[] props) => (object)6).Execute();
    }
    [Fact]
    public void PosTest_CreateShips()
    {
        Dictionary<string, IUObject> ships = new();

        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Game.Get.UObjects", (object[] props) => (object)ships).Execute();

        var act = new CreateShips();

        act.Execute();

        Assert.True(ships.ToList().Count == 6);
    }
}
