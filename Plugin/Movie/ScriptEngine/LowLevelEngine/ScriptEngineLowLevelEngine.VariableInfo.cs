using System.Collections.Generic;

namespace UniTASPlugin.Movie.ScriptEngine.ValueTypes;

public partial class ScriptEngineLowLevelEngine
{
    private class VariableInfo
    {
        public string Name { get; }
        public List<ValueType> Value { get; }
        public int ScopeIndex { get; }

        public VariableInfo(string name, List<ValueType> value, int scopeIndex)
        {
            Name = name;
            Value = value;
            ScopeIndex = scopeIndex;
        }
    }
}