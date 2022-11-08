namespace UniTASPlugin.Movie.ScriptEngine.OpCodes.Logic;

public class AndOpCode : LogicComparisonBase
{
    public AndOpCode(RegisterType dest, RegisterType left, RegisterType right) : base(dest, left, right)
    {
    }
}