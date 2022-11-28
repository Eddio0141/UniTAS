using System.Collections.Generic;
using UniTASPlugin.Movie.ScriptEngine.ValueTypes;

namespace UniTASPlugin.Movie.ScriptEngine.EngineMethods;

public abstract class EngineExternalMethodBase
{
    public string Name { get; }
    public int ArgCount { get; }
    public bool ReturnsValue { get; }

    protected EngineExternalMethodBase(string name, int argCount = 0, bool returnsValue = false)
    {
        Name = name;
        ArgCount = argCount;
        ReturnsValue = returnsValue;
    }

    public abstract List<ValueType> Invoke(IEnumerable<ValueType> args);
}