namespace UniTASPlugin.Movie.ScriptEngine.OpCodes.VariableSet;

public class NewVariableOpCode : OpCodeBase
{
    public string Name { get; }
    public RegisterType Register { get; }

    public NewVariableOpCode(RegisterType register, string name)
    {
        Register = register;
        Name = name;
    }
}