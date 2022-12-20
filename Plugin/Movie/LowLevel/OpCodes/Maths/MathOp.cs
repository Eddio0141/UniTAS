using UniTASPlugin.Movie.LowLevel.Register;

namespace UniTASPlugin.Movie.LowLevel.OpCodes.Maths;

public abstract class MathOp : OpCode
{
    public RegisterType Result { get; }
    public RegisterType Left { get; }
    public RegisterType Right { get; }

    protected MathOp(RegisterType result, RegisterType left, RegisterType right)
    {
        Result = result;
        Left = left;
        Right = right;
    }
}