using UniTASPlugin.Movie.LowLevel.Register;

namespace UniTASPlugin.Movie.LowLevel.OpCodes.BitwiseOps;

public class BitwiseShiftRightOpCode : Bitwise
{
    public BitwiseShiftRightOpCode(RegisterType resultRegister, RegisterType left, RegisterType right) : base(
        resultRegister, left, right)
    {
    }
}