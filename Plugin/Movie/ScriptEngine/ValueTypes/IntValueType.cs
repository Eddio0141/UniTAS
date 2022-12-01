using System.Collections.Generic;

namespace UniTASPlugin.Movie.ScriptEngine.ValueTypes;

public class IntValueType : ValueType
{
    public int Value { get; }

    public IntValueType(int value)
    {
        Value = value;
    }

    public override string ToString()
    {
        return Value.ToString();
    }

    public override object Clone()
    {
        return new IntValueType(Value);
    }
}