namespace UniTASPlugin.Movie.ScriptEngine.OpCodes.Jump;

public class JumpIfGtEqOpCode : JumpCompareBase
{
    public JumpIfGtEqOpCode(int offset, RegisterType left, RegisterType right) : base(offset, left, right)
    {
    }
}