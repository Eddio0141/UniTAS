using UniTASPlugin.Movie.LowLevel.Register;

namespace UniTASPlugin.Movie.LowLevel.OpCodes.Maths;

public class DivOpCode : MathOp
{
    public DivOpCode(RegisterType result, RegisterType left, RegisterType right) : base(result, left, right)
    {
    }
}