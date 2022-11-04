namespace UniTASPlugin.Movie.ScriptEngine.OpCodes.RegisterSet;

public abstract class RegisterSetBase : OpCodeBase
{
    public RegisterType Register { get; }

    protected RegisterSetBase(RegisterType register)
    {
        Register = register;
    }
}