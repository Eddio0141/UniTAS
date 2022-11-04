namespace UniTASPlugin.Movie.ScriptEngine.OpCodes.BitwiseOps;

public abstract class BitwiseBase : OpCodeBase
{
    public RegisterType ResultRegister { get; }
    public RegisterType Left { get; }
    public RegisterType Right { get; }

    protected BitwiseBase(RegisterType resultRegister, RegisterType left, RegisterType right)
    {
        ResultRegister = resultRegister;
        Left = left;
        Right = right;
    }
}