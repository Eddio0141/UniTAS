using UniTASPlugin.Movie.MovieRunner.LowLevel.Register;

namespace UniTASPlugin.Movie.MovieRunner.LowLevel.OpCodes.Tuple;

public class ClearTupleOpCode : OpCode
{
    public RegisterType Register { get; }

    public ClearTupleOpCode(RegisterType register)
    {
        Register = register;
    }
}