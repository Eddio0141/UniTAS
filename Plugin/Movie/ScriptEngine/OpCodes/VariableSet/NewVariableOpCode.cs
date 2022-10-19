namespace UniTASPlugin.Movie.ScriptEngine.OpCodes.VariableSet;

public class NewVariableOpCode : OpCodeBase
{
    public string Name { get; }
    public RegisterType Register { get; }
}