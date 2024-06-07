using Hwdtech;

namespace SpaceBattle.Lib;

public class SetFuel : ICommand
{
    public IUObject patient;

    public SetFuel(IUObject patient)
    {
        this.patient = patient;
    }

    public void Execute()
    {
        var fuel = IoC.Resolve<int>("Game.IniFuelIter.Next");
        IoC.Resolve<ICommand>("Game.UObject.Set", patient, "fuel", fuel).Execute();
    }
}
