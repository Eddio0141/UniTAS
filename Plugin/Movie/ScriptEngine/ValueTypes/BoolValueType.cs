namespace UniTASPlugin.Movie.ScriptEngine.ValueTypes;

public class BoolValueType : ValueType
{
    public bool Value { get; }

    public BoolValueType(bool value)
    {
        Value = value;
    }
}