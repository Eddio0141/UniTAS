using UniTASPlugin.Movie.Model.Script.LowLevel.ValueTypes;

namespace UniTASPlugin.Movie.Model.Script.LowLevel.OpCodes.RegisterSet;

public class ConstToRegisterOpCode : RegisterSetBase
{
    public ValueType Value { get; }
}