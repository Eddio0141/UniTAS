using UniTASPlugin.Movie.LowLevel.Register;

namespace UniTASPlugin.Movie.LowLevel.OpCodes.BitwiseOps;

public class BitwiseShiftLeftOpCode : Bitwise
{
    public BitwiseShiftLeftOpCode(RegisterType resultRegister, RegisterType left, RegisterType right) : base(
        resultRegister, left, right)
    {
    }
}