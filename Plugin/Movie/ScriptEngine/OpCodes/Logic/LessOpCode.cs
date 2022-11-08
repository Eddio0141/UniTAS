namespace UniTASPlugin.Movie.ScriptEngine.OpCodes.Logic;

public class LessOpCode : LogicComparisonBase
{
    public LessOpCode(RegisterType dest, RegisterType left, RegisterType right) : base(dest, left, right)
    {
    }
}