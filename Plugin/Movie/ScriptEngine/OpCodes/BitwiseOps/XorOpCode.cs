namespace UniTASPlugin.Movie.ScriptEngine.OpCodes.BitwiseOps;

public class XorOpCode : BitwiseBase
{
    public XorOpCode(RegisterType resultRegister, RegisterType left, RegisterType right) : base(resultRegister, left, right)
    {
    }
}