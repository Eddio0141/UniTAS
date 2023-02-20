using System;

namespace UniTAS.Plugin.Movie.ValueTypes;

public abstract class ValueType : ICloneable
{
    public abstract object Clone();
    public abstract override string ToString();
}