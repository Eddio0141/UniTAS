using UniTAS.Plugin.Movie.LowLevel.Register;

namespace UniTAS.Plugin.Movie.LowLevel.OpCodes.RegisterSet;

public class MoveOpCode : RegisterSet
{
    public RegisterType Dest { get; }

    public MoveOpCode(RegisterType source, RegisterType dest) : base(source)
    {
        Dest = dest;
    }
}