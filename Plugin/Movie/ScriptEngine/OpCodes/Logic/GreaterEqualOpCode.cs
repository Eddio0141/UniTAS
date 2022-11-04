namespace UniTASPlugin.Movie.ScriptEngine.OpCodes.Logic;

public class GreaterEqualOpCode : LogicComparisonBase
{
    public GreaterEqualOpCode(RegisterType resultRegister, RegisterType left, RegisterType right) : base(resultRegister, left, right)
    {
    }
}