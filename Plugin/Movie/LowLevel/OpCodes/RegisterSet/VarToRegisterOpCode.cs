using UniTASPlugin.Movie.LowLevel.Register;

namespace UniTASPlugin.Movie.LowLevel.OpCodes.RegisterSet;

public class VarToRegisterOpCode : RegisterSet
{
    public string Name { get; }

    public VarToRegisterOpCode(RegisterType register, string name) : base(register)
    {
        Name = name;
    }
}