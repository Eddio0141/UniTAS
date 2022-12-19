using UniTASPlugin.Movie.MovieRunner.LowLevel.Register;

namespace UniTASPlugin.Movie.MovieRunner.LowLevel.OpCodes.Maths;

public class DivOpCode : MathOp
{
    public DivOpCode(RegisterType result, RegisterType left, RegisterType right) : base(result, left, right)
    {
    }
}