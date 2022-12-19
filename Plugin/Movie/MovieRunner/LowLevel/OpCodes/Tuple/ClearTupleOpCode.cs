namespace UniTASPlugin.Movie.ScriptEngine.OpCodes.Tuple;

public class ClearTupleOpCode : OpCode
{
    public RegisterType Register { get; }

    public ClearTupleOpCode(RegisterType register)
    {
        Register = register;
    }
}