namespace UniTASPlugin.Movie.ScriptEngine.OpCodes.Logic;

public abstract class LogicComparisonBase : LogicBase
{
    public RegisterType Left { get; }
    public RegisterType Right { get; }

    protected LogicComparisonBase(RegisterType resultRegister, RegisterType left, RegisterType right) : base(resultRegister)
    {
        Left = left;
        Right = right;
    }
}