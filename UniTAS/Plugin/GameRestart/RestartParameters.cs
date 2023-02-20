using UniTAS.Plugin.FixedUpdateSync;
using UniTAS.Plugin.GameEnvironment;
using UniTAS.Plugin.GameRestart.EventInterfaces;
using UniTAS.Plugin.Logger;
using UniTAS.Plugin.MonoBehaviourController;
using UniTAS.Plugin.StaticFieldStorage;
using UniTAS.Plugin.Trackers.DontDestroyOnLoadTracker;
using UniTAS.Plugin.UnitySafeWrappers.Interfaces;

namespace UniTAS.Plugin.GameRestart;

public class RestartParameters
{
    public VirtualEnvironment VirtualEnvironment { get; }
    public ISyncFixedUpdate SyncFixedUpdate { get; }
    public IUnityWrapper UnityWrapper { get; }
    public IMonoBehaviourController MonoBehaviourController { get; }
    public ILogger Logger { get; }
    public IOnGameRestart[] OnGameRestart { get; }
    public IStaticFieldManipulator StaticFieldManipulator { get; }
    public IDontDestroyOnLoadInfo DontDestroyOnLoadInfo { get; }
    public IOnGameRestartResume[] OnGameRestartResume { get; }
    public IOnPreGameRestart[] OnPreGameRestart { get; }

    public RestartParameters(VirtualEnvironment virtualEnvironment, ISyncFixedUpdate syncFixedUpdate,
        IUnityWrapper unityWrapper, IMonoBehaviourController monoBehaviourController, ILogger logger,
        IOnGameRestart[] onGameRestart, IStaticFieldManipulator staticFieldManipulator,
        IDontDestroyOnLoadInfo dontDestroyOnLoadInfo, IOnGameRestartResume[] onGameRestartResume,
        IOnPreGameRestart[] onPreGameRestart)

    {
        VirtualEnvironment = virtualEnvironment;
        SyncFixedUpdate = syncFixedUpdate;
        UnityWrapper = unityWrapper;
        MonoBehaviourController = monoBehaviourController;
        Logger = logger;
        OnGameRestart = onGameRestart;
        StaticFieldManipulator = staticFieldManipulator;
        DontDestroyOnLoadInfo = dontDestroyOnLoadInfo;
        OnGameRestartResume = onGameRestartResume;
        OnPreGameRestart = onPreGameRestart;
    }
}