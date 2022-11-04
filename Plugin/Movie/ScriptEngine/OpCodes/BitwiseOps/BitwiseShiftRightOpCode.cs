namespace UniTASPlugin.Movie.ScriptEngine.OpCodes.BitwiseOps;

public class BitwiseShiftRightOpCode : BitwiseBase
{
    public BitwiseShiftRightOpCode(RegisterType resultRegister, RegisterType left, RegisterType right) : base(resultRegister, left, right)
    {
    }
}