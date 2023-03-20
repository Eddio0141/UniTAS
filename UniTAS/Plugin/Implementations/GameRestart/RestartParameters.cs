using System.Diagnostics.CodeAnalysis;
using UniTAS.Plugin.Interfaces.Events.SoftRestart;
using UniTAS.Plugin.Services;
using UniTAS.Plugin.Services.Logging;
using UniTAS.Plugin.Services.UnitySafeWrappers.Wrappers;

namespace UniTAS.Plugin.Implementations.GameRestart;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class RestartParameters
{
    public ISyncFixedUpdate SyncFixedUpdate { get; }
    public IUnityWrapper UnityWrapper { get; }
    public IMonoBehaviourController MonoBehaviourController { get; }
    public ILogger Logger { get; }
    public IOnGameRestart[] OnGameRestart { get; }
    public IStaticFieldManipulator StaticFieldManipulator { get; }
    public IOnGameRestartResume[] OnGameRestartResume { get; }
    public IOnPreGameRestart[] OnPreGameRestart { get; }

    public RestartParameters(ISyncFixedUpdate syncFixedUpdate,
        IUnityWrapper unityWrapper, IMonoBehaviourController monoBehaviourController, ILogger logger,
        IOnGameRestart[] onGameRestart, IStaticFieldManipulator staticFieldManipulator,
        IOnGameRestartResume[] onGameRestartResume,
        IOnPreGameRestart[] onPreGameRestart)

    {
        SyncFixedUpdate = syncFixedUpdate;
        UnityWrapper = unityWrapper;
        MonoBehaviourController = monoBehaviourController;
        Logger = logger;
        OnGameRestart = onGameRestart;
        StaticFieldManipulator = staticFieldManipulator;
        OnGameRestartResume = onGameRestartResume;
        OnPreGameRestart = onPreGameRestart;
    }
}