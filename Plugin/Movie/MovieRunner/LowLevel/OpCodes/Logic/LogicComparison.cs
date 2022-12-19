namespace UniTASPlugin.Movie.ScriptEngine.OpCodes.Logic;

public abstract class LogicComparison : Logic
{
    public RegisterType Left { get; }
    public RegisterType Right { get; }

    protected LogicComparison(RegisterType dest, RegisterType left, RegisterType right) : base(dest)
    {
        Left = left;
        Right = right;
    }
}