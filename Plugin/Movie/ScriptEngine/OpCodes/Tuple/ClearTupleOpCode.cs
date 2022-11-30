namespace UniTASPlugin.Movie.ScriptEngine.OpCodes.Tuple;

public class ClearTupleOpCode : OpCodeBase
{
    public RegisterType Register { get; }

    public ClearTupleOpCode(RegisterType register)
    {
        Register = register;
    }
}