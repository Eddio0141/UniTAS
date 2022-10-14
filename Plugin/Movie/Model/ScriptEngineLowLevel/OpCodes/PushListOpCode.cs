namespace UniTASPlugin.Movie.Model.ScriptEngineLowLevel.OpCodes;

public class PushListOpCode : OpCodeBase
{
    public RegisterType Dest { get; }
    public RegisterType Source { get; }
}