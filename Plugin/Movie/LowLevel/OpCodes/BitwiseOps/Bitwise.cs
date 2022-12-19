using UniTASPlugin.Movie.MovieRunner.LowLevel.Register;

namespace UniTASPlugin.Movie.MovieRunner.LowLevel.OpCodes.BitwiseOps;

public abstract class Bitwise : OpCode
{
    public RegisterType ResultRegister { get; }
    public RegisterType Left { get; }
    public RegisterType Right { get; }

    protected Bitwise(RegisterType resultRegister, RegisterType left, RegisterType right)
    {
        ResultRegister = resultRegister;
        Left = left;
        Right = right;
    }
}