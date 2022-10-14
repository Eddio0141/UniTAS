namespace UniTASPlugin.Movie.Model.Script.LowLevel.OpCodes.VariableSet;

public class NewVariableOpCode : OpCodeBase
{
    public string Name { get; }
    public RegisterType Register { get; }
}