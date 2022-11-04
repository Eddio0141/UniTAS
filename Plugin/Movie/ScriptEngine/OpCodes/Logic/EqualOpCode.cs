namespace UniTASPlugin.Movie.ScriptEngine.OpCodes.Logic;

public class EqualOpCode : LogicComparisonBase
{
    public EqualOpCode(RegisterType resultRegister, RegisterType left, RegisterType right) : base(resultRegister, left, right)
    {
    }
}