using UniTASPlugin.Movie.LowLevel.Register;

namespace UniTASPlugin.Movie.LowLevel.OpCodes.Tuple;

public class ClearTupleOpCode : OpCode
{
    public RegisterType Register { get; }

    public ClearTupleOpCode(RegisterType register)
    {
        Register = register;
    }
}