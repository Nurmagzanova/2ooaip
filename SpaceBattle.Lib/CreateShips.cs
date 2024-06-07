using Hwdtech;

namespace SpaceBattle.Lib;

public class CreateShips : ICommand
{
    public void Execute()
    {
        var allShips = IoC.Resolve<int>("Game.NumOfAllShips");
        var ships = IoC.Resolve<Dictionary<string, IUObject>>("Game.Get.UObjects");
        for (var i = 0; i < allShips; i++)
        {
            var id = Guid.NewGuid().ToString();
            ships[id] = IoC.Resolve<IUObject>("Game.Ship.Create");
        }
    }
}
