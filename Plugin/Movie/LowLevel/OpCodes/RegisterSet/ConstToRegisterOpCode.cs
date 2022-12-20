using UniTASPlugin.Movie.LowLevel.Register;
using UniTASPlugin.Movie.ValueTypes;

namespace UniTASPlugin.Movie.LowLevel.OpCodes.RegisterSet;

public class ConstToRegisterOpCode : RegisterSet
{
    public ValueType Value { get; }

    public ConstToRegisterOpCode(RegisterType register, ValueType value) : base(register)
    {
        Value = value;
    }
}