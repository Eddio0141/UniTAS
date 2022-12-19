namespace UniTASPlugin.Movie.ScriptEngine.OpCodes.Logic;

public class LessOpCode : LogicComparison
{
    public LessOpCode(RegisterType dest, RegisterType left, RegisterType right) : base(dest, left, right)
    {
    }
}