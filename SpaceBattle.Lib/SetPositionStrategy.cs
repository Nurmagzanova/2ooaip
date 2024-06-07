namespace SpaceBattle.Lib;

public class SetPositionStrategy : IStrategy
{
    public object Run(params object[] args)
    {
        var patient = (IUObject)args[0];
        return new SetPosition(patient);
    }
}
