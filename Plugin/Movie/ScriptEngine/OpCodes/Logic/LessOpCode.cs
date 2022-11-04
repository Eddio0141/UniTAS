namespace UniTASPlugin.Movie.ScriptEngine.OpCodes.Logic;

public class LessOpCode : LogicComparisonBase
{
    public LessOpCode(RegisterType resultRegister, RegisterType left, RegisterType right) : base(resultRegister, left, right)
    {
    }
}