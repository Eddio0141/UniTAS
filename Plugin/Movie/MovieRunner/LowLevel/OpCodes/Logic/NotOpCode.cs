namespace UniTASPlugin.Movie.ScriptEngine.OpCodes.Logic;

public class NotOpCode : Logic
{
    public RegisterType Source { get; }

    public NotOpCode(RegisterType dest, RegisterType source) : base(dest)
    {
        Source = source;
    }
}