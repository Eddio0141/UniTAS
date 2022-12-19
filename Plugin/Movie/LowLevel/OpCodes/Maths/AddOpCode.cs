using UniTASPlugin.Movie.MovieRunner.LowLevel.Register;

namespace UniTASPlugin.Movie.MovieRunner.LowLevel.OpCodes.Maths;

public class AddOpCode : MathOp
{
    public AddOpCode(RegisterType result, RegisterType left, RegisterType right) : base(result, left, right)
    {
    }
}