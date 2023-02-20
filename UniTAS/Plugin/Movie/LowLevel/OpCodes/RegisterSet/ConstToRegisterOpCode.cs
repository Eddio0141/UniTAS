using UniTAS.Plugin.Movie.LowLevel.Register;
using UniTAS.Plugin.Movie.ValueTypes;

namespace UniTAS.Plugin.Movie.LowLevel.OpCodes.RegisterSet;

public class ConstToRegisterOpCode : RegisterSet
{
    public ValueType Value { get; }

    public ConstToRegisterOpCode(RegisterType register, ValueType value) : base(register)
    {
        Value = value;
    }
}