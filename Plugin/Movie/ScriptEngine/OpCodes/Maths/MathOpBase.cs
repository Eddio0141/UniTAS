namespace UniTASPlugin.Movie.ScriptEngine.OpCodes.Maths;

public abstract class MathOpBase : OpCodeBase
{
    public RegisterType Result { get; }
    public RegisterType Left { get; }
    public RegisterType Right { get; }

    protected MathOpBase(RegisterType result, RegisterType left, RegisterType right)
    {
        Result = result;
        Left = left;
        Right = right;
    }
}