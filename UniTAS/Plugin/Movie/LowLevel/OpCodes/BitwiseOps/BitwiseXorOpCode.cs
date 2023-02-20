using UniTAS.Plugin.Movie.LowLevel.Register;

namespace UniTAS.Plugin.Movie.LowLevel.OpCodes.BitwiseOps;

public class BitwiseXorOpCode : Bitwise
{
    public BitwiseXorOpCode(RegisterType resultRegister, RegisterType left, RegisterType right) : base(resultRegister,
        left, right)
    {
    }
}