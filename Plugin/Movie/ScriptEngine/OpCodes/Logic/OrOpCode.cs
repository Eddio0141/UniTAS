namespace UniTASPlugin.Movie.ScriptEngine.OpCodes.Logic;

public class OrOpCode : LogicComparisonBase
{
    public OrOpCode(RegisterType dest, RegisterType left, RegisterType right) : base(dest, left, right)
    {
    }
}