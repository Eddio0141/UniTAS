namespace UniTASPlugin.Movie.ScriptEngine.OpCodes.StackOp;

public class PushStackOpCode : OpCodeBase
{
    public RegisterType Register { get; }

    public PushStackOpCode(RegisterType register)
    {
        Register = register;
    }
}