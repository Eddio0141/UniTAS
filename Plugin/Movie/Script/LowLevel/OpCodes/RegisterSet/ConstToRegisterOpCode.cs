using UniTASPlugin.Movie.Script.LowLevel.ValueTypes;

namespace UniTASPlugin.Movie.Script.LowLevel.OpCodes.RegisterSet;

public class ConstToRegisterOpCode : RegisterSetBase
{
    public ValueType Value { get; }
}