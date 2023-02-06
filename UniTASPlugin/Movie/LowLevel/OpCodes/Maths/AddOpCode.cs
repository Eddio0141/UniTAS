using UniTASPlugin.Movie.LowLevel.Register;

namespace UniTASPlugin.Movie.LowLevel.OpCodes.Maths;

public class AddOpCode : MathOp
{
    public AddOpCode(RegisterType result, RegisterType left, RegisterType right) : base(result, left, right)
    {
    }
}