using System.Diagnostics.CodeAnalysis;
using UniTAS.Plugin.Interfaces.Events.SoftRestart;
using UniTAS.Plugin.Services;
using UniTAS.Plugin.Services.Logging;
using UniTAS.Plugin.Services.UnitySafeWrappers.Wrappers;

namespace UniTAS.Plugin.Implementations.GameRestart;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class RestartParameters
{
    public ISyncFixedUpdateCycle SyncFixedUpdateCycle { get; }
    public ISceneWrapper SceneWrapper { get; }
    public IMonoBehaviourController MonoBehaviourController { get; }
    public ILogger Logger { get; }
    public IOnGameRestart[] OnGameRestart { get; }
    public IStaticFieldManipulator StaticFieldManipulator { get; }
    public IOnGameRestartResume[] OnGameRestartResume { get; }
    public IOnPreGameRestart[] OnPreGameRestart { get; }

    public RestartParameters(ISyncFixedUpdateCycle syncFixedUpdateCycle,
        ISceneWrapper sceneWrapper, IMonoBehaviourController monoBehaviourController, ILogger logger,
        IOnGameRestart[] onGameRestart, IStaticFieldManipulator staticFieldManipulator,
        IOnGameRestartResume[] onGameRestartResume,
        IOnPreGameRestart[] onPreGameRestart)

    {
        SyncFixedUpdateCycle = syncFixedUpdateCycle;
        SceneWrapper = sceneWrapper;
        MonoBehaviourController = monoBehaviourController;
        Logger = logger;
        OnGameRestart = onGameRestart;
        StaticFieldManipulator = staticFieldManipulator;
        OnGameRestartResume = onGameRestartResume;
        OnPreGameRestart = onPreGameRestart;
    }
}