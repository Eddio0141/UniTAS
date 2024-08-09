using System.Threading;
using HarmonyLib;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Loaders;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.Logging;
using UnityEngine;

namespace UniTAS.Patcher.Implementations;

[Singleton]
[ForceInstantiate]
public class LiveScripting : ILiveScripting
{
    private readonly Thread _setupThread;
    private readonly ILogger _logger;

    public LiveScripting(ILogger logger)
    {
        _logger = logger;
        _setupThread = new(() =>
        {
            var allTypes = AccessTools.GetTypesFromAssembly(typeof(LiveScripting).Assembly);
            foreach (var type in allTypes)
            {
                UserData.RegisterType(type);
            }

            UserData.RegisterType<Vector4>();
            UserData.RegisterType<Vector3>();
            UserData.RegisterType<Vector2>();
        });
        _setupThread.Start();
    }

    public Script NewScript()
    {
        if (_setupThread.ThreadState == ThreadState.Running)
            _setupThread.Join();

        var script = new Script(CoreModules.Preset_Complete)
        {
            Options =
            {
                // do NOT use unity loader
                ScriptLoader = new FileSystemScriptLoader(),
                DebugInput = _ => null,
                DebugPrint = _logger.LogInfo,
            }
        };

        return script;
    }
}