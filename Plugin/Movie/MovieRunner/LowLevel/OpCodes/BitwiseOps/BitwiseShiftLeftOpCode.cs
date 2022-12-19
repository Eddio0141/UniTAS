using UniTASPlugin.Movie.MovieRunner.LowLevel.Register;

namespace UniTASPlugin.Movie.MovieRunner.LowLevel.OpCodes.BitwiseOps;

public class BitwiseShiftLeftOpCode : Bitwise
{
    public BitwiseShiftLeftOpCode(RegisterType resultRegister, RegisterType left, RegisterType right) : base(
        resultRegister, left, right)
    {
    }
}