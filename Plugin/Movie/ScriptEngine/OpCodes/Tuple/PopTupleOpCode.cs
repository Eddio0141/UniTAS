namespace UniTASPlugin.Movie.ScriptEngine.OpCodes.Tuple;

public class PopTupleOpCode : OpCodeBase
{
    public RegisterType Dest { get; }
    public RegisterType Source { get; }

    public PopTupleOpCode(RegisterType dest, RegisterType source)
    {
        Dest = dest;
        Source = source;
    }
}