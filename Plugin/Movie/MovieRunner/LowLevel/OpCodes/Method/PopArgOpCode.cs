namespace UniTASPlugin.Movie.ScriptEngine.OpCodes.Method;

public class PopArgOpCode : OpCode
{
    public RegisterType Register { get; }

    public PopArgOpCode(RegisterType register)
    {
        Register = register;
    }
}