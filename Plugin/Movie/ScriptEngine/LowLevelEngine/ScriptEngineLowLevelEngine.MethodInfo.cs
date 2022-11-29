using System.Collections.Generic;

namespace UniTASPlugin.Movie.ScriptEngine.ValueTypes;

public partial class ScriptEngineLowLevelEngine
{
    private class MethodInfo
    {
        public int Pc { get; set; }
        public int MethodIndex { get; }
        public List<VariableInfo> Vars { get; set; } = new();

        public MethodInfo(int pc, int methodIndex)
        {
            Pc = pc;
            MethodIndex = methodIndex;
        }
    }
}