using UniTASPlugin.Movie.MovieRunner.LowLevel.Register;

namespace UniTASPlugin.Movie.MovieRunner.LowLevel.OpCodes;

public class SetVariableOpCode : OpCode
{
    public string Name { get; }
    public RegisterType Register { get; }

    public SetVariableOpCode(RegisterType register, string name)
    {
        Register = register;
        Name = name;
    }
}