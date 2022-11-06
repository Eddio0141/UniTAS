namespace UniTASPlugin.Movie.ScriptEngine.OpCodes.RegisterSet;

public class MoveOpCode : RegisterSetBase
{
    public RegisterType Dest { get; }

    public MoveOpCode(RegisterType source, RegisterType dest) : base(source)
    {
        Dest = dest;
    }
}