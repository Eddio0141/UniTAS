using System.Reflection;
using HarmonyLib;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.Logging;

namespace UniTAS.Patcher.Implementations.GameRestart;

[Register]
[ForceInstantiate]
public class GOGGalaxyStopOnRestart
{
    private readonly ILogger _logger;
    private readonly IGameRestart _gameRestart;
    private readonly MethodInfo shutdown;
    public GOGGalaxyStopOnRestart(ILogger logger, IGameRestart gameRestart)
    {
        var galaxyManager = AccessTools.TypeByName("Galaxy.Api.GalaxyInstance");

        if (galaxyManager == null)
            return;
        
        _logger = logger;
        _gameRestart = gameRestart;

        shutdown = AccessTools.Method(galaxyManager, "Shutdown");

        if (shutdown != null)
            _gameRestart.OnPreGameRestart += Deinit;
    }

    public void Deinit()
    {
        _logger.LogDebug("Deinitializing GOG Galaxy");
        shutdown.Invoke(null, [true]);
    }
}