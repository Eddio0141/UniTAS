namespace UniTASPlugin.Movie.ScriptEngine.OpCodes.Logic;

public class GreaterOpCode : LogicComparisonBase
{
    public GreaterOpCode(RegisterType resultRegister, RegisterType left, RegisterType right) : base(resultRegister, left, right)
    {
    }
}