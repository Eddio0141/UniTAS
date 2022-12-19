namespace UniTASPlugin.Movie.ScriptEngine.OpCodes.StackOp;

public class PopStackOpCode : OpCode
{
    public RegisterType Register { get; }

    public PopStackOpCode(RegisterType register)
    {
        Register = register;
    }
}