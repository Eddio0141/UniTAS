namespace UniTASPlugin.Movie.ScriptEngine.OpCodes.BitwiseOps;

public class BitwiseAndOpCode : BitwiseBase
{
    public BitwiseAndOpCode(RegisterType resultRegister, RegisterType left, RegisterType right) : base(resultRegister, left, right)
    {
    }
}