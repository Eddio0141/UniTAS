namespace UniTASPlugin.Movie.Script.LowLevel.OpCodes.Maths;

public abstract class MathOpBase : OpCodeBase
{
    public RegisterType Result { get; }
    public RegisterType Left { get; }
    public RegisterType Right { get; }
}