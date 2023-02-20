using UniTAS.Plugin.Movie.LowLevel.Register;

namespace UniTAS.Plugin.Movie.LowLevel.OpCodes.Tuple;

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