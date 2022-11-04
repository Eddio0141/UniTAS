namespace UniTASPlugin.Movie.ScriptEngine.OpCodes.BitwiseOps;

public class BitwiseShiftLeftOpCode : BitwiseBase
{
    public BitwiseShiftLeftOpCode(RegisterType resultRegister, RegisterType left, RegisterType right) : base(resultRegister, left, right)
    {
    }
}