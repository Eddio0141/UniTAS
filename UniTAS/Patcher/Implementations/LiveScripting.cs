using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Loaders;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.Logging;

namespace UniTAS.Patcher.Implementations;

[Singleton]
public class LiveScripting : ILiveScripting
{
    private readonly ILogger _logger;

    public LiveScripting(ILogger logger)
    {
        _logger = logger;
    }

    public Script NewScript()
    {
        return new(CoreModules.Preset_Complete)
        {
            Options =
            {
                // do NOT use unity loader
                ScriptLoader = new FileSystemScriptLoader(),
                DebugInput = _ => null,
                DebugPrint = _logger.LogInfo
            }
        };
    }
}