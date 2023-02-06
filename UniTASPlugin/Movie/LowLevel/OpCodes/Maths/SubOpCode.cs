using UniTASPlugin.Movie.LowLevel.Register;

namespace UniTASPlugin.Movie.LowLevel.OpCodes.Maths;

public class SubOpCode : MathOp
{
    public SubOpCode(RegisterType result, RegisterType left, RegisterType right) : base(result, left, right)
    {
    }
}