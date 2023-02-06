namespace UniTASPlugin.Movie.ValueTypes;

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