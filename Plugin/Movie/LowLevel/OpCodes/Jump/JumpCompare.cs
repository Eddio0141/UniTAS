using UniTASPlugin.Movie.MovieRunner.LowLevel.Register;

namespace UniTASPlugin.Movie.MovieRunner.LowLevel.OpCodes.Jump;

public abstract class JumpCompare : Jump
{
    public RegisterType Left { get; }
    public RegisterType Right { get; }

    protected JumpCompare(int offset, RegisterType left, RegisterType right) : base(offset)
    {
        Left = left;
        Right = right;
    }
}