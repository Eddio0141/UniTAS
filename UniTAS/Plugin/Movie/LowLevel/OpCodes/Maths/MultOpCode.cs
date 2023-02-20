using UniTAS.Plugin.Movie.LowLevel.Register;

namespace UniTAS.Plugin.Movie.LowLevel.OpCodes.Maths;

public class MultOpCode : MathOp
{
    public MultOpCode(RegisterType result, RegisterType left, RegisterType right) : base(result, left, right)
    {
    }
}