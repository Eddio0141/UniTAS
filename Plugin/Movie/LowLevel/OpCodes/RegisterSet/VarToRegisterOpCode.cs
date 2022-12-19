using UniTASPlugin.Movie.MovieRunner.LowLevel.Register;

namespace UniTASPlugin.Movie.MovieRunner.LowLevel.OpCodes.RegisterSet;

public class VarToRegisterOpCode : RegisterSet
{
    public string Name { get; }

    public VarToRegisterOpCode(RegisterType register, string name) : base(register)
    {
        Name = name;
    }
}