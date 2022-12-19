using UniTASPlugin.Movie.MovieRunner.LowLevel.Register;

namespace UniTASPlugin.Movie.MovieRunner.LowLevel.OpCodes.StackOp;

public class PushStackOpCode : OpCode
{
    public RegisterType Register { get; }

    public PushStackOpCode(RegisterType register)
    {
        Register = register;
    }
}