using UniTAS.Plugin.Movie.LowLevel.Register;

namespace UniTAS.Plugin.Movie.LowLevel.OpCodes.StackOp;

public class PopStackOpCode : OpCode
{
    public RegisterType Register { get; }

    public PopStackOpCode(RegisterType register)
    {
        Register = register;
    }
}