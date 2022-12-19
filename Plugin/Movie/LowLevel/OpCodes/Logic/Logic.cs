using UniTASPlugin.Movie.LowLevel.Register;

namespace UniTASPlugin.Movie.LowLevel.OpCodes.Logic;

public abstract class Logic : OpCode
{
    public RegisterType Dest { get; }

    protected Logic(RegisterType dest)
    {
        Dest = dest;
    }
}