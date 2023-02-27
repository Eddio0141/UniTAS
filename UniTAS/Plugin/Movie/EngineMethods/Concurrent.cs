using MoonSharp.Interpreter;

namespace UniTAS.Plugin.Movie.EngineMethods;

public class Concurrent : EngineMethodClass
{
    public override string ClassName => "concurrent";

    public int Register(DynValue coroutine, bool preUpdate, params DynValue[] defaultArgs)
    {
        // TODO
        return 0;
    }
}