
using Hwdtech;

namespace SpaceBattle.Lib;

public class SetPosition : ICommand
{
    public IUObject patient;

    public SetPosition(IUObject patient)
    {
        this.patient = patient;
    }

    public void Execute()
    {
        var coords = IoC.Resolve<Vector>("Game.IniPosIter.Next");
        IoC.Resolve<ICommand>("Game.UObject.Set", patient, "position", coords).Execute();
    }
}
