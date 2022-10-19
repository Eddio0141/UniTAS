namespace UniTASPlugin.Movie.ScriptEngine.OpCodes;

public class PushListOpCode : OpCodeBase
{
    public RegisterType Dest { get; }
    public RegisterType Source { get; }
}