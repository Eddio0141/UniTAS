namespace UniTASPlugin.Movie.ScriptEngine.OpCodes.Jump;

public class JumpIfLtEqOpCode : JumpCompareBase
{
    public JumpIfLtEqOpCode(int offset, RegisterType left, RegisterType right) : base(offset, left, right)
    {
    }
}