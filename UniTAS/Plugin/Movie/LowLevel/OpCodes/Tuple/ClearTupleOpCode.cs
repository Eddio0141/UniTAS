using UniTAS.Plugin.Movie.LowLevel.Register;

namespace UniTAS.Plugin.Movie.LowLevel.OpCodes.Tuple;

public class ClearTupleOpCode : OpCode
{
    public RegisterType Register { get; }

    public ClearTupleOpCode(RegisterType register)
    {
        Register = register;
    }
}