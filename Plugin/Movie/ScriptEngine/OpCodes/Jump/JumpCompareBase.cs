namespace UniTASPlugin.Movie.ScriptEngine.OpCodes.Jump;

public abstract class JumpCompareBase : JumpBase
{
    public RegisterType Left { get; }
    public RegisterType Right { get; }

    protected JumpCompareBase(int offset, RegisterType left, RegisterType right) : base(offset)
    {
        Left = left;
        Right = right;
    }
}