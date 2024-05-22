namespace SpaceBattle.Lib;

public class GetObject : IStrategyRenamed
{
    public object Strategy(params object[] param)
    {
        var gameItemId = (string)param[1];
        var objects = (Dictionary<string, object>)param[0];
        return objects[gameItemId];
    }
}