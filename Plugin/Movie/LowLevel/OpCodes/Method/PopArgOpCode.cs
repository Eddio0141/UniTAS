using UniTASPlugin.Movie.LowLevel.Register;

namespace UniTASPlugin.Movie.LowLevel.OpCodes.Method;

public class PopArgOpCode : OpCode
{
    public RegisterType Register { get; }

    public PopArgOpCode(RegisterType register)
    {
        Register = register;
    }
}