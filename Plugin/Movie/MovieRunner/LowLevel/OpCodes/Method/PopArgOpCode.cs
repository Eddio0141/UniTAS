using UniTASPlugin.Movie.MovieRunner.LowLevel.Register;

namespace UniTASPlugin.Movie.MovieRunner.LowLevel.OpCodes.Method;

public class PopArgOpCode : OpCode
{
    public RegisterType Register { get; }

    public PopArgOpCode(RegisterType register)
    {
        Register = register;
    }
}