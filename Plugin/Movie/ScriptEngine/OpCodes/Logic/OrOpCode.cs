namespace UniTASPlugin.Movie.ScriptEngine.OpCodes.Logic;

public class OrOpCode : LogicComparisonBase
{
    public OrOpCode(RegisterType resultRegister, RegisterType left, RegisterType right) : base(resultRegister, left, right)
    {
    }
}