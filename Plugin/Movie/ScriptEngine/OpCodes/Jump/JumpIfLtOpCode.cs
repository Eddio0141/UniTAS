namespace UniTASPlugin.Movie.ScriptEngine.OpCodes.Jump;

public class JumpIfLtOpCode : JumpCompareBase
{
    public JumpIfLtOpCode(int offset, RegisterType left, RegisterType right) : base(offset, left, right)
    {
    }
}