using System.Collections.Generic;
using UniTASPlugin.Movie.ScriptEngine.ValueTypes;

namespace UniTASPlugin.Movie.ScriptEngine;

public class Register
{
    private ValueType _innerValue;
    private List<ValueType> _tupleValues;

    public ValueType InnerValue
    {
        get => _innerValue;
        set
        {
            _innerValue = value;
            IsTuple = false;
        }
    }

    public bool IsTuple { get; private set; }

    public List<ValueType> TupleValues
    {
        get => _tupleValues;
        set
        {
            _tupleValues = value;
            IsTuple = true;
        }
    }
}