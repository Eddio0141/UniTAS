using UniTAS.Plugin.Movie.LowLevel.Register;

namespace UniTAS.Plugin.Movie.LowLevel.OpCodes.RegisterSet;

public class VarToRegisterOpCode : RegisterSet
{
    public string Name { get; }

    public VarToRegisterOpCode(RegisterType register, string name) : base(register)
    {
        Name = name;
    }
}