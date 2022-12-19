namespace UniTASPlugin.Movie.ScriptEngine.OpCodes.RegisterSet;

public abstract class RegisterSet : OpCode
{
    public RegisterType Register { get; }

    protected RegisterSet(RegisterType register)
    {
        Register = register;
    }
}