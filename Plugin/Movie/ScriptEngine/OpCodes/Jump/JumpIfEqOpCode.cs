namespace UniTASPlugin.Movie.ScriptEngine.OpCodes.Jump;

public class JumpIfEqOpCode : JumpCompareBase
{
    public JumpIfEqOpCode(int offset, RegisterType left, RegisterType right) : base(offset, left, right)
    {
    }
}