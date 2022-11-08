namespace UniTASPlugin.Movie.ScriptEngine.OpCodes.Logic;

public class LessEqualOpCode : LogicComparisonBase
{
    public LessEqualOpCode(RegisterType dest, RegisterType left, RegisterType right) : base(dest, left, right)
    {
    }
}