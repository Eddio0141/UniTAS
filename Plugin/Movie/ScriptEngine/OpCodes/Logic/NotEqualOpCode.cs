namespace UniTASPlugin.Movie.ScriptEngine.OpCodes.Logic;

public class NotEqualOpCode : LogicComparisonBase
{
    public NotEqualOpCode(RegisterType resultRegister, RegisterType left, RegisterType right) : base(resultRegister, left, right)
    {
    }
}