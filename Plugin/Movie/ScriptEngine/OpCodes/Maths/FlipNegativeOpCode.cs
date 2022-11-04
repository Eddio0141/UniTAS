namespace UniTASPlugin.Movie.ScriptEngine.OpCodes.Maths;

public class FlipNegativeOpCode : OpCodeBase
{
    public RegisterType Dest { get; }
    public RegisterType Source { get; }

    public FlipNegativeOpCode(RegisterType source, RegisterType dest)
    {
        Source = source;
        Dest = dest;
    }
}