using UniTASPlugin.Movie.Model.ScriptEngineLowLevel.ValueTypes;

namespace UniTASPlugin.Movie.Model.ScriptEngineLowLevel.OpCodes.RegisterSet;

public class ConstToRegisterOpCode : RegisterSetBase
{
    public ValueType Value { get; }
}