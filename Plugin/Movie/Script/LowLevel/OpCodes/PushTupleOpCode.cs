namespace UniTASPlugin.Movie.Script.LowLevel.OpCodes;

public class PushTupleOpCode : OpCodeBase
{
    public RegisterType Dest { get; }
    public RegisterType Source { get; }
}