namespace UniTASPlugin.Movie.ScriptEngine.OpCodes.Jump;

public class JumpIfGtOpCode : JumpCompareBase
{
    public JumpIfGtOpCode(int offset, RegisterType left, RegisterType right) : base(offset, left, right)
    {
    }
}