namespace UniTASPlugin.Movie.ScriptEngine.OpCodes.Method;

public class GotoMethodOpCode : OpCode
{
    public string MethodName { get; }

    public GotoMethodOpCode(string methodName)
    {
        MethodName = methodName;
    }
}