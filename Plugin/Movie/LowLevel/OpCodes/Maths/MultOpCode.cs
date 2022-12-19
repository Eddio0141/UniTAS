using UniTASPlugin.Movie.LowLevel.Register;

namespace UniTASPlugin.Movie.LowLevel.OpCodes.Maths;

public class MultOpCode : MathOp
{
    public MultOpCode(RegisterType result, RegisterType left, RegisterType right) : base(result, left, right)
    {
    }
}