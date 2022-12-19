namespace UniTASPlugin.Movie.MovieRunner.LowLevel.OpCodes.Method;

public class GotoMethodOpCode : OpCode
{
    public string MethodName { get; }

    public GotoMethodOpCode(string methodName)
    {
        MethodName = methodName;
    }
}