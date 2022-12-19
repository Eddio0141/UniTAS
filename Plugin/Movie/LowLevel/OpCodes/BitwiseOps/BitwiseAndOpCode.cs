using UniTASPlugin.Movie.MovieRunner.LowLevel.Register;

namespace UniTASPlugin.Movie.MovieRunner.LowLevel.OpCodes.BitwiseOps;

public class BitwiseAndOpCode : Bitwise
{
    public BitwiseAndOpCode(RegisterType resultRegister, RegisterType left, RegisterType right) : base(resultRegister,
        left, right)
    {
    }
}