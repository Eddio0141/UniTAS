namespace UniTASPlugin.Movie.ScriptEngine.OpCodes.RegisterSet;

public class MoveOpCode : RegisterSetBase
{
    public RegisterType RegisterDest { get; }

    public MoveOpCode(RegisterType register, RegisterType registerDest) : base(register)
    {
        RegisterDest = registerDest;
    }
}