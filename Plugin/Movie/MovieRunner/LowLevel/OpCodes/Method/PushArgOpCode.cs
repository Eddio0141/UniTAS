using UniTASPlugin.Movie.MovieRunner.LowLevel.Register;

namespace UniTASPlugin.Movie.MovieRunner.LowLevel.OpCodes.Method;

public class PushArgOpCode : OpCode
{
    public RegisterType RegisterType { get; }

    public PushArgOpCode(RegisterType registerType)
    {
        RegisterType = registerType;
    }
}