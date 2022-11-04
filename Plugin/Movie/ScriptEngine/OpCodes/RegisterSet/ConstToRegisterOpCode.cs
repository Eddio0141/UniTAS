using UniTASPlugin.Movie.ScriptEngine.ValueTypes;

namespace UniTASPlugin.Movie.ScriptEngine.OpCodes.RegisterSet;

public class ConstToRegisterOpCode : RegisterSetBase
{
    public ValueType Value { get; }

    public ConstToRegisterOpCode(RegisterType register, ValueType value) : base(register)
    {
        Value = value;
    }
}