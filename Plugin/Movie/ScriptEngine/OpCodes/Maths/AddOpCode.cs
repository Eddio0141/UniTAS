namespace UniTASPlugin.Movie.ScriptEngine.OpCodes.Maths;

public class AddOpCode : MathOpBase
{
    public AddOpCode(RegisterType result, RegisterType left, RegisterType right) : base(result, left, right)
    {
    }
}