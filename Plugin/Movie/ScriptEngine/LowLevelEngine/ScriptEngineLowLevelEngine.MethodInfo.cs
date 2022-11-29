namespace UniTASPlugin.Movie.ScriptEngine.ValueTypes;

public partial class ScriptEngineLowLevelEngine
{
    private class MethodInfo
    {
        public int Pc { get; set; }
        public int MethodIndex { get; }

        public MethodInfo(int pc, int methodIndex)
        {
            Pc = pc;
            MethodIndex = methodIndex;
        }
    }
}