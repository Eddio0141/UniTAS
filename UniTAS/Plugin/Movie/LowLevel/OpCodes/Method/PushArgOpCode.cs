using UniTAS.Plugin.Movie.LowLevel.Register;

namespace UniTAS.Plugin.Movie.LowLevel.OpCodes.Method;

public class PushArgOpCode : OpCode
{
    public RegisterType RegisterType { get; }

    public PushArgOpCode(RegisterType registerType)
    {
        RegisterType = registerType;
    }
}