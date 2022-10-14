namespace UniTASPlugin.Movie.Model.Script.LowLevel.OpCodes;

public class PushListOpCode : OpCodeBase
{
    public RegisterType Dest { get; }
    public RegisterType Source { get; }
}