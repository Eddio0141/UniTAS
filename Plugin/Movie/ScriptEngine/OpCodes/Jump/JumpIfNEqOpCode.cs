namespace UniTASPlugin.Movie.ScriptEngine.OpCodes.Jump;

public class JumpIfNEqOpCode : JumpCompareBase
{
    public JumpIfNEqOpCode(int offset, RegisterType left, RegisterType right) : base(offset, left, right)
    {
    }
}