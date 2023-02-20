using UniTAS.Plugin.Movie.LowLevel.Register;

namespace UniTAS.Plugin.Movie.LowLevel.OpCodes.Maths;

public class FlipNegativeOpCode : OpCode
{
    public RegisterType Dest { get; }
    public RegisterType Source { get; }

    public FlipNegativeOpCode(RegisterType source, RegisterType dest)
    {
        Source = source;
        Dest = dest;
    }
}