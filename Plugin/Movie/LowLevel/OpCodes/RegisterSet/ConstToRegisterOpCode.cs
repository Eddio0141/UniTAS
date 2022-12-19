using UniTASPlugin.Movie.MovieRunner.LowLevel.Register;
using UniTASPlugin.Movie.MovieRunner.ValueTypes;

namespace UniTASPlugin.Movie.MovieRunner.LowLevel.OpCodes.RegisterSet;

public class ConstToRegisterOpCode : RegisterSet
{
    public ValueType Value { get; }

    public ConstToRegisterOpCode(RegisterType register, ValueType value) : base(register)
    {
        Value = value;
    }
}