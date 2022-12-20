using UniTASPlugin.Movie.LowLevel.Register;

namespace UniTASPlugin.Movie.LowLevel.OpCodes.Tuple;

public class PopTupleOpCode : OpCode
{
    public RegisterType Dest { get; }
    public RegisterType Source { get; }

    public PopTupleOpCode(RegisterType dest, RegisterType source)
    {
        Dest = dest;
        Source = source;
    }
}