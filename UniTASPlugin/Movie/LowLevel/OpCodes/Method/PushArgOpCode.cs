using UniTASPlugin.Movie.LowLevel.Register;

namespace UniTASPlugin.Movie.LowLevel.OpCodes.Method;

public class PushArgOpCode : OpCode
{
    public RegisterType RegisterType { get; }

    public PushArgOpCode(RegisterType registerType)
    {
        RegisterType = registerType;
    }
}