using System.Collections.Generic;
using UniTASPlugin.Movie.MovieRunner.ValueTypes;

namespace UniTASPlugin.Movie.MovieRunner.LowLevel;

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