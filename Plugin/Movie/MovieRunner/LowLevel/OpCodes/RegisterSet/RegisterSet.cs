using UniTASPlugin.Movie.MovieRunner.LowLevel.Register;

namespace UniTASPlugin.Movie.MovieRunner.LowLevel.OpCodes.RegisterSet;

public abstract class RegisterSet : OpCode
{
    public RegisterType Register { get; }

    protected RegisterSet(RegisterType register)
    {
        Register = register;
    }
}