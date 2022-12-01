using System.Collections.Generic;

namespace UniTASPlugin.Movie.ScriptEngine.ValueTypes;

public class BoolValueType : ValueType
{
    public bool Value { get; }

    public BoolValueType(bool value)
    {
        Value = value;
    }

    public override string ToString()
    {
        return Value.ToString();
    }

    public override object Clone()
    {
        return new BoolValueType(Value);
    }
}