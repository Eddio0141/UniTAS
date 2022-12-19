namespace UniTASPlugin.Movie.ScriptEngine.OpCodes.Logic;

public abstract class Logic : OpCode
{
    public RegisterType Dest { get; }

    protected Logic(RegisterType dest)
    {
        Dest = dest;
    }
}