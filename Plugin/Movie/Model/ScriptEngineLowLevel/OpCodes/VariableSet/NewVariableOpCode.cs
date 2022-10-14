namespace UniTASPlugin.Movie.Model.ScriptEngineLowLevel.OpCodes.VariableSet;

public class NewVariableOpCode : OpCodeBase
{
    public string Name { get; }
    public RegisterType Register { get; }
}