using UniTASPlugin.FixedUpdateSync;
using UniTASPlugin.GameEnvironment;
using UniTASPlugin.GameRestart.EventInterfaces;
using UniTASPlugin.Logger;
using UniTASPlugin.MonoBehaviourController;
using UniTASPlugin.StaticFieldStorage;
using UniTASPlugin.Trackers.DontDestroyOnLoadTracker;
using UniTASPlugin.UnitySafeWrappers.Interfaces;

namespace UniTASPlugin.GameRestart;

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