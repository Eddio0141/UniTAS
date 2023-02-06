using UniTASPlugin.Movie.LowLevel.Register;

namespace UniTASPlugin.Movie.LowLevel.OpCodes.StackOp;

public class PushStackOpCode : OpCode
{
    public RegisterType Register { get; }

    public PushStackOpCode(RegisterType register)
    {
        Register = register;
    }
}