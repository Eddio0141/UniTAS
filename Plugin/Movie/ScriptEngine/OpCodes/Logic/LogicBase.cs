namespace UniTASPlugin.Movie.ScriptEngine.OpCodes.Logic;

public abstract class LogicBase : OpCodeBase
{
    public RegisterType ResultRegister { get; }

    protected LogicBase(RegisterType resultRegister)
    {
        ResultRegister = resultRegister;
    }
}