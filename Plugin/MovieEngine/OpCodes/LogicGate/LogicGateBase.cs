namespace UniTASPlugin.MovieEngine.OpCodes.LogicGate;

public abstract class LogicGateBase : OpCodeBase
{
    public RegisterType ResultRegister { get; }
    public RegisterType SourceRegister { get; }
    public RegisterType SourceRegister2 { get; }
}