using System.Collections.Generic;
using UniTAS.Plugin.Movie.ValueTypes;

namespace UniTAS.Plugin.Movie.EngineMethods;

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

    public abstract List<ValueType> Invoke(IEnumerable<IEnumerable<ValueType>> args, MovieRunner runner);
}