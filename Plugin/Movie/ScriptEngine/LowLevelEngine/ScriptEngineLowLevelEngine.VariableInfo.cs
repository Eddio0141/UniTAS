using System.Collections.Generic;

namespace UniTASPlugin.Movie.ScriptEngine.ValueTypes;

public partial class ScriptEngineLowLevelEngine
{
    private class VariableInfo
    {
        public string Name { get; }
        public List<ValueType> Value { get; set; }

        public VariableInfo(string name, List<ValueType> value)
        {
            Name = name;
            Value = value;
        }
    }
}