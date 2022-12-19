namespace UniTASPlugin.Movie.ScriptEngine.OpCodes.Logic;

public class EqualOpCode : LogicComparison
{
    public EqualOpCode(RegisterType dest, RegisterType left, RegisterType right) : base(dest, left, right)
    {
    }
}