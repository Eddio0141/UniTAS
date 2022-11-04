namespace UniTASPlugin.Movie.ScriptEngine.ValueTypes;

public class FloatValueType : ValueType
{
    public float Value { get; }

    public FloatValueType(float value)
    {
        Value = value;
    }
}