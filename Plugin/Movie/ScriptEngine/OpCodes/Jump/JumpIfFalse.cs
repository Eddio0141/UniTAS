namespace UniTASPlugin.Movie.ScriptEngine.OpCodes.Jump;

public class JumpIfFalse : JumpBase
{
    public RegisterType Register { get; }

    public JumpIfFalse(int offset, RegisterType register) : base(offset)
    {
        Register = register;
    }
}