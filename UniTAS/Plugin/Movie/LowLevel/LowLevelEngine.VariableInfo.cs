using System.Collections.Generic;
using UniTAS.Plugin.Movie.ValueTypes;

namespace UniTAS.Plugin.Movie.LowLevel;

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