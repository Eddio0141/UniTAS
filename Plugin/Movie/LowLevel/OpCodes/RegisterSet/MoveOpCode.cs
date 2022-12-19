using UniTASPlugin.Movie.MovieRunner.LowLevel.Register;

namespace UniTASPlugin.Movie.MovieRunner.LowLevel.OpCodes.RegisterSet;

public class MoveOpCode : RegisterSet
{
    public RegisterType Dest { get; }

    public MoveOpCode(RegisterType source, RegisterType dest) : base(source)
    {
        Dest = dest;
    }
}