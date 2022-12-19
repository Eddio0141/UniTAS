namespace UniTASPlugin.Movie.ScriptEngine.OpCodes.Logic;

public class GreaterEqualOpCode : LogicComparison
{
    public GreaterEqualOpCode(RegisterType dest, RegisterType left, RegisterType right) : base(dest, left, right)
    {
    }
}