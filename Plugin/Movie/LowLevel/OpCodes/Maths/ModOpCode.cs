using UniTASPlugin.Movie.LowLevel.Register;

namespace UniTASPlugin.Movie.LowLevel.OpCodes.Maths;

public class ModOpCode : MathOp
{
    public ModOpCode(RegisterType result, RegisterType left, RegisterType right) : base(result, left, right)
    {
    }
}