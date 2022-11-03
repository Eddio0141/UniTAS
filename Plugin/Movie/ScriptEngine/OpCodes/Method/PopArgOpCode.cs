namespace UniTASPlugin.Movie.ScriptEngine.OpCodes.Method;

public class PopArgOpCode : OpCodeBase
{
    public RegisterType Register { get; }

    public PopArgOpCode(RegisterType register)
    {
        Register = register;
    }
}