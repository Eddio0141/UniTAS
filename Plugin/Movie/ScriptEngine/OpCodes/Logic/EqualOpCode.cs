namespace UniTASPlugin.Movie.ScriptEngine.OpCodes.Logic;

public class EqualOpCode : LogicComparisonBase
{
    public EqualOpCode(RegisterType dest, RegisterType left, RegisterType right) : base(dest, left, right)
    {
    }
}