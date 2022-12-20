using UniTASPlugin.Movie.LowLevel.Register;

namespace UniTASPlugin.Movie.LowLevel.OpCodes.RegisterSet;

public abstract class RegisterSet : OpCode
{
    public RegisterType Register { get; }

    protected RegisterSet(RegisterType register)
    {
        Register = register;
    }
}