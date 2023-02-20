using UniTAS.Plugin.Movie.LowLevel.Register;

namespace UniTAS.Plugin.Movie.LowLevel.OpCodes.Logic;

public abstract class Logic : OpCode
{
    public RegisterType Dest { get; }

    protected Logic(RegisterType dest)
    {
        Dest = dest;
    }
}