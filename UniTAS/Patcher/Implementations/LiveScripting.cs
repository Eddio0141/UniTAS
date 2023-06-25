using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Loaders;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.Logging;

namespace UniTAS.Patcher.Implementations;

[Singleton]
public class LiveScripting : ILiveScripting
{
    private readonly Script _script;

    public LiveScripting(ILogger logger)
    {
        _script = new(CoreModules.Preset_Complete)
        {
            Options =
            {
                // do NOT use unity loader
                ScriptLoader = new FileSystemScriptLoader(),
                DebugInput = _ => null,
                DebugPrint = logger.LogInfo
            }
        };
    }

    public void Evaluate(string code)
    {
        _script.DoString(code);
    }
}