namespace UniTASPlugin.Movie.ScriptEngine.OpCodes;

public class PushTupleOpCode : OpCodeBase
{
    public RegisterType Dest { get; }
    public RegisterType Source { get; }

    public PushTupleOpCode(RegisterType dest, RegisterType source)
    {
        Dest = dest;
        Source = source;
    }
}