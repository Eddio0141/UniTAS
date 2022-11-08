namespace UniTASPlugin.Movie.ScriptEngine.OpCodes.Logic;

public abstract class LogicComparisonBase : LogicBase
{
    public RegisterType Left { get; }
    public RegisterType Right { get; }

    protected LogicComparisonBase(RegisterType dest, RegisterType left, RegisterType right) : base(dest)
    {
        Left = left;
        Right = right;
    }
}