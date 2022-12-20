using UniTASPlugin.Movie.LowLevel.Register;

namespace UniTASPlugin.Movie.LowLevel.OpCodes.StackOp;

public class PopStackOpCode : OpCode
{
    public RegisterType Register { get; }

    public PopStackOpCode(RegisterType register)
    {
        Register = register;
    }
}