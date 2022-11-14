namespace UniTASPlugin.Movie.ScriptEngine.OpCodes.Jump;

public class JumpIfTrue : JumpBase
{
    public RegisterType Register { get; }

    public JumpIfTrue(int offset, RegisterType register) : base(offset)
    {
        Register = register;
    }
}