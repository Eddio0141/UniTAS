using UniTAS.Plugin.Movie.LowLevel.Register;

namespace UniTAS.Plugin.Movie.LowLevel.OpCodes.BitwiseOps;

public class BitwiseOrOpCode : Bitwise
{
    public BitwiseOrOpCode(RegisterType resultRegister, RegisterType left, RegisterType right) : base(resultRegister,
        left, right)
    {
    }
}