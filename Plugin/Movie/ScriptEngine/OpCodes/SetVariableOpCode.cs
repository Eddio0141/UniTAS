namespace UniTASPlugin.Movie.ScriptEngine.OpCodes;

public class SetVariableOpCode : OpCodeBase
{
    public string Name { get; }
    public RegisterType Register { get; }

    public SetVariableOpCode(string name, RegisterType register)
    {
        Name = name;
        Register = register;
    }
}