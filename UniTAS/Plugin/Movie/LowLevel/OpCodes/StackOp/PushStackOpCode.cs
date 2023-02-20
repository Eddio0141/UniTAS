using UniTAS.Plugin.Movie.LowLevel.Register;

namespace UniTAS.Plugin.Movie.LowLevel.OpCodes.StackOp;

public class PushStackOpCode : OpCode
{
    public RegisterType Register { get; }

    public PushStackOpCode(RegisterType register)
    {
        Register = register;
    }
}