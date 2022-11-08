namespace UniTASPlugin.Movie.ScriptEngine.OpCodes.Logic;

public abstract class LogicBase : OpCodeBase
{
    public RegisterType Dest { get; }

    protected LogicBase(RegisterType dest)
    {
        Dest = dest;
    }
}