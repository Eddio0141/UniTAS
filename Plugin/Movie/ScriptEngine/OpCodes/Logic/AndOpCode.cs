namespace UniTASPlugin.Movie.ScriptEngine.OpCodes.Logic;

public class AndOpCode : LogicComparisonBase
{
    public AndOpCode(RegisterType resultRegister, RegisterType left, RegisterType right) : base(resultRegister, left, right)
    {
    }
}