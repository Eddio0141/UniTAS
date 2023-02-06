using System.Collections.Generic;
using UniTASPlugin.Movie.ValueTypes;

namespace UniTASPlugin.Movie.LowLevel;

public partial class LowLevelEngine
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