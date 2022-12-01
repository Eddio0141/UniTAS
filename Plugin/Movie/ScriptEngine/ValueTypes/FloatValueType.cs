using System.Collections.Generic;

namespace UniTASPlugin.Movie.ScriptEngine.ValueTypes;

public class FloatValueType : ValueType
{
    public float Value { get; }

    public FloatValueType(float value)
    {
        Value = value;
    }

    public override string ToString()
    {
        return Value.ToString();
    }

    public override object Clone()
    {
        return new FloatValueType(Value);
    }
}