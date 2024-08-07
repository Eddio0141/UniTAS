using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Loaders;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.Logging;

namespace UniTAS.Patcher.Implementations;

[Singleton]
public class LiveScripting(ILogger logger) : ILiveScripting
{
    public Script NewScript()
    {
        var script = new Script(CoreModules.Preset_Complete)
        {
            Options =
            {
                // do NOT use unity loader
                ScriptLoader = new FileSystemScriptLoader(),
                DebugInput = _ => null,
                DebugPrint = logger.LogInfo,
            }
        };

        return script;
    }
}