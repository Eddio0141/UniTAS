using System;
using System.Collections.Generic;
using ValueType = UniTASPlugin.Movie.ValueTypes.ValueType;

namespace UniTASPlugin.Movie.LowLevel.Register;

public class Register : ICloneable
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

    public List<ValueType> TupleValues { get; set; } = new();

    public object Clone()
    {
        var register = new Register();
        if (IsTuple)
        {
            register.IsTuple = true;
            register.TupleValues = new List<ValueType>();
            foreach (var tupleValue in TupleValues)
            {
                register.TupleValues.Add((ValueType)tupleValue.Clone());
            }
        }
        else if (InnerValue != null)
        {
            register.InnerValue = (ValueType)InnerValue.Clone();
        }

        return register;
    }
}