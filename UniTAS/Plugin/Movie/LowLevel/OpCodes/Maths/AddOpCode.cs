using UniTAS.Plugin.Movie.LowLevel.Register;

namespace UniTAS.Plugin.Movie.LowLevel.OpCodes.Maths;

public class AddOpCode : MathOp
{
    public AddOpCode(RegisterType result, RegisterType left, RegisterType right) : base(result, left, right)
    {
    }
}