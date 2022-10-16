namespace UniTASPlugin.MovieEngine.OpCodes.Jump;

public abstract class JumpCompareBase : JumpBase
{
    public RegisterType Left { get; }
    public RegisterType Right { get; }
}