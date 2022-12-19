namespace UniTASPlugin.Movie.ScriptEngine.OpCodes.StackOp;

public class PushStackOpCode : OpCode
{
    public RegisterType Register { get; }

    public PushStackOpCode(RegisterType register)
    {
        Register = register;
    }
}