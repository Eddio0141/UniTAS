using UniTASPlugin.Movie.MovieRunner.LowLevel.Register;

namespace UniTASPlugin.Movie.MovieRunner.LowLevel.OpCodes.Tuple;

public class PushTupleOpCode : OpCode
{
    public RegisterType Dest { get; }
    public RegisterType Source { get; }

    public PushTupleOpCode(RegisterType dest, RegisterType source)
    {
        Dest = dest;
        Source = source;
    }
}