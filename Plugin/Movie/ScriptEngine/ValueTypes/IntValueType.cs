namespace UniTASPlugin.Movie.ScriptEngine.ValueTypes;

public class IntValueType : ValueType
{
    public int Value { get; }

    public IntValueType(int value)
    {
        Value = value;
    }
}