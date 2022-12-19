namespace UniTASPlugin.Movie.ScriptEngine.OpCodes.Method;

public class PushArgOpCode : OpCode
{
    public RegisterType RegisterType { get; }

    public PushArgOpCode(RegisterType registerType)
    {
        RegisterType = registerType;
    }
}