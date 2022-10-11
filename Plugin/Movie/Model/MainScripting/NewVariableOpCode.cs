using UniTASPlugin.Movie.Model.MainScripting.ValueTypes;

namespace UniTASPlugin.Movie.Model.MainScripting;

/// <summary>
/// Creates a new variable, with an initial value from the temporary register.
/// </summary>
public class NewVariableOpCode : OpCodeBase
{
    public string Name { get; }
}