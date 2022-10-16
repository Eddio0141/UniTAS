using UniTASPlugin.MovieEngine.ValueTypes;

namespace UniTASPlugin.MovieEngine.OpCodes.RegisterSet;

public class ConstToRegisterOpCode : RegisterSetBase
{
    public ValueType Value { get; }
}