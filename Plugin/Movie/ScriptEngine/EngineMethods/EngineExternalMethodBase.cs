using System.Collections.Generic;
using UniTASPlugin.Movie.ScriptEngine.ValueTypes;

namespace UniTASPlugin.Movie.ScriptEngine.EngineMethods;

public abstract class EngineExternalMethodBase
{
    public string Name { get; }
    public int ArgCount { get; }

    protected EngineExternalMethodBase(string name, int argCount)
    {
        Name = name;
        ArgCount = argCount;
    }

    public abstract ValueType Invoke(ICollection<ValueType> args);
}