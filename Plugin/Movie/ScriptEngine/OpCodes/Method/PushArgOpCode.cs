namespace UniTASPlugin.Movie.ScriptEngine.OpCodes.Method;

public class PushArgOpCode : OpCodeBase
{
    public RegisterType RegisterType { get; }

    public PushArgOpCode(RegisterType registerType)
    {
        RegisterType = registerType;
    }
}