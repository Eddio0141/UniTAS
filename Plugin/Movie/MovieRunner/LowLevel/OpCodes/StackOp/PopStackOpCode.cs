using UniTASPlugin.Movie.MovieRunner.LowLevel.Register;

namespace UniTASPlugin.Movie.MovieRunner.LowLevel.OpCodes.StackOp;

public class PopStackOpCode : OpCode
{
    public RegisterType Register { get; }

    public PopStackOpCode(RegisterType register)
    {
        Register = register;
    }
}