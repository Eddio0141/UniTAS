namespace UniTASPlugin.Movie.ScriptEngine.OpCodes.BitwiseOps;

public class BitwiseOrOpCode : BitwiseBase
{
    public BitwiseOrOpCode(RegisterType resultRegister, RegisterType left, RegisterType right) : base(resultRegister,
        left, right)
    {
    }
}