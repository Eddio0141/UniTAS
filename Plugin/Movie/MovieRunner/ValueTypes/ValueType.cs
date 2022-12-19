using System;

namespace UniTASPlugin.Movie.ScriptEngine.ValueTypes;

public abstract class ValueType : ICloneable
{
    public abstract object Clone();
    public abstract override string ToString();
}