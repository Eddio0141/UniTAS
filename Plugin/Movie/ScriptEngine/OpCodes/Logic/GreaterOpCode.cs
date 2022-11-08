namespace UniTASPlugin.Movie.ScriptEngine.OpCodes.Logic;

public class GreaterOpCode : LogicComparisonBase
{
    public GreaterOpCode(RegisterType dest, RegisterType left, RegisterType right) : base(dest, left, right)
    {
    }
}