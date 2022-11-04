namespace UniTASPlugin.Movie.ScriptEngine.OpCodes.BitwiseOps;

public class BitwiseXorOpCode : BitwiseBase
{
    public BitwiseXorOpCode(RegisterType resultRegister, RegisterType left, RegisterType right) : base(resultRegister, left, right)
    {
    }
}