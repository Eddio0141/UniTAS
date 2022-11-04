namespace UniTASPlugin.Movie.ScriptEngine.OpCodes.Logic;

public class LessEqualOpCode : LogicComparisonBase
{
    public LessEqualOpCode(RegisterType resultRegister, RegisterType left, RegisterType right) : base(resultRegister, left, right)
    {
    }
}