using System.Collections.Generic;
using UniTASPlugin.Movie.ScriptEngine.ValueTypes;

namespace UniTASPlugin.Movie.ScriptEngine.EngineMethods;

public abstract class EngineExternalMethod
{
    public string Name { get; }
    public int ArgCount { get; }
    public int ArgReturnCount { get; }

    protected EngineExternalMethod(string name, int argCount = 0, int argReturnCount = 0)
    {
        Name = name;
        ArgCount = argCount;
        ArgReturnCount = argReturnCount;
    }

    public abstract List<ValueType> Invoke(IEnumerable<IEnumerable<ValueType>> args);
}