using UniTAS.Plugin.Movie.LowLevel.Register;

namespace UniTAS.Plugin.Movie.LowLevel.OpCodes.Method;

public class PopArgOpCode : OpCode
{
    public RegisterType Register { get; }

    public PopArgOpCode(RegisterType register)
    {
        Register = register;
    }
}