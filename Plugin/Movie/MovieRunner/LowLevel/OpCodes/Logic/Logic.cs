using UniTASPlugin.Movie.MovieRunner.LowLevel.Register;

namespace UniTASPlugin.Movie.MovieRunner.LowLevel.OpCodes.Logic;

public abstract class Logic : OpCode
{
    public RegisterType Dest { get; }

    protected Logic(RegisterType dest)
    {
        Dest = dest;
    }
}