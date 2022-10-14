namespace UniTASPlugin.Movie.Model.ScriptEngineLowLevel.OpCodes;

public class PushTupleOpCode : OpCodeBase
{
    public RegisterType Dest { get; }
    public RegisterType Source { get; }
}