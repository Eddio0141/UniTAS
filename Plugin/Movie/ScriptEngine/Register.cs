using System.Collections.Generic;
using UniTASPlugin.Movie.ScriptEngine.ValueTypes;

namespace UniTASPlugin.Movie.ScriptEngine;

public class Register
{
    private ValueType _innerValue;

    public ValueType InnerValue
    {
        get => _innerValue;
        set
        {
            _innerValue = value;
            IsTuple = false;
        }
    }

    public bool IsTuple { get; set; }

    public List<ValueType> TupleValues { get; set; }
}