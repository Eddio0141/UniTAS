namespace UniTASPlugin.Movie.ScriptEngine.OpCodes.Logic;

public class GreaterOpCode : LogicComparison
{
    public GreaterOpCode(RegisterType dest, RegisterType left, RegisterType right) : base(dest, left, right)
    {
    }
}