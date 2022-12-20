using System;

namespace UniTASPlugin.Movie.ValueTypes;

public abstract class ValueType : ICloneable
{
    public abstract object Clone();
    public abstract override string ToString();
}