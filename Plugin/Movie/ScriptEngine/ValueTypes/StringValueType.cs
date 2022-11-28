namespace UniTASPlugin.Movie.ScriptEngine.ValueTypes;

public class StringValueType : ValueType
{
    public string Value { get; }

    public StringValueType(string value)
    {
        Value = value;
    }

    public override string ToString()
    {
        return Value;
    }
}