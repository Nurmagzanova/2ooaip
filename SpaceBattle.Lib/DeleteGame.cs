using Hwdtech;

namespace SpaceBattle.Lib;
public class Delete : ICommand
{
    private readonly string gameId;
    public Delete(string gameId)
    {
        this.gameId = gameId;
    }
    public void Execute()
    {
        var scopeMap = IoC.Resolve<Dictionary<string, object>>("ScopeMap");
        scopeMap.Remove(gameId);
    }
}