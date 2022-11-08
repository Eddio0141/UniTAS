namespace UniTASPlugin.Movie.ScriptEngine.OpCodes.Logic;

public class GreaterEqualOpCode : LogicComparisonBase
{
    public GreaterEqualOpCode(RegisterType dest, RegisterType left, RegisterType right) : base(dest, left, right)
    {
    }
}