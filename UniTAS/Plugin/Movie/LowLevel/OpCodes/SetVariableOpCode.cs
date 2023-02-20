using UniTAS.Plugin.Movie.LowLevel.Register;

namespace UniTAS.Plugin.Movie.LowLevel.OpCodes;

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