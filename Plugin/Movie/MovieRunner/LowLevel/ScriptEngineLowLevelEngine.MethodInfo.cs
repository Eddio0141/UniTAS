using System.Collections.Generic;

namespace UniTASPlugin.Movie.MovieRunner.LowLevel;

public partial class ScriptEngineLowLevelEngine
{
    private class MethodInfo
    {
        public int Pc { get; set; }
        public int MethodIndex { get; }
        public Stack<List<VariableInfo>> Vars { get; set; }

        public MethodInfo(int pc, int methodIndex, Stack<List<VariableInfo>> vars)
        {
            Pc = pc;
            MethodIndex = methodIndex;
            Vars = vars;
        }
    }
}