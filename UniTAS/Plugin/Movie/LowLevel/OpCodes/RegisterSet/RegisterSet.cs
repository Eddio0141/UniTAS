using UniTAS.Plugin.Movie.LowLevel.Register;

namespace UniTAS.Plugin.Movie.LowLevel.OpCodes.RegisterSet;

public abstract class RegisterSet : OpCode
{
    public RegisterType Register { get; }

    protected RegisterSet(RegisterType register)
    {
        Register = register;
    }
}