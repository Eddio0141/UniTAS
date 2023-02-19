using System;
using BepInEx;
using UniTASPlugin.GameEnvironment;
using UniTASPlugin.GameRestart;
using UniTASPlugin.GameRestart.EventInterfaces;
using UniTASPlugin.Logger;
using UniTASPlugin.ReverseInvoker;
using Object = UnityEngine.Object;

namespace UniTASPlugin.GameInitialRestart;

// ReSharper disable once ClassNeverInstantiated.Global
public class GameInitialRestart : GameRestart.GameRestart, IGameInitialRestart, IOnPreGameRestart, IOnGameRestart,
    IOnGameRestartResume
{
    private readonly ILogger _logger;
    private readonly VirtualEnvironment _virtualEnvironment;

    private bool _restartOperationStarted;
    public bool FinishedRestart { get; private set; }

    private readonly PatchReverseInvoker _reverseInvoker;

    public GameInitialRestart(RestartParameters restartParameters, PatchReverseInvoker reverseInvoker) :
        base(restartParameters)
    {
        _reverseInvoker = reverseInvoker;
        _logger = restartParameters.Logger;
        _virtualEnvironment = restartParameters.VirtualEnvironment;
    }

    public void OnPreGameRestart()
    {
        if (!_restartOperationStarted) return;

        // TODO fix this hack
        _virtualEnvironment.RunVirtualEnvironment = true;
        _virtualEnvironment.FrameTime = 0.001f;
    }

    public void OnGameRestart(DateTime startupTime, bool preSceneLoad)
    {
        if (preSceneLoad || !_restartOperationStarted) return;

        // TODO fix this hack
        _virtualEnvironment.FrameTime = 0f;
    }

    public void InitialRestart()
    {
        if (_restartOperationStarted) return;
        _restartOperationStarted = true;

        var time = _reverseInvoker.Invoke(() => DateTime.Now);
        SoftRestart(time);
    }

    protected override void DestroyGameObjects()
    {
        var allObjects = Object.FindObjectsOfType(typeof(Object));
        _logger.LogDebug($"Attempting destruction of {allObjects.Length} objects");
        foreach (var obj in allObjects)
        {
            if (obj is BaseUnityPlugin)
            {
                _logger.LogDebug($"Found BepInEx type: {obj.GetType().FullName}, skipping");
                continue;
            }

            try
            {
                // if (obj is MonoBehaviour monoBehaviour)
                // {
                //     monoBehaviour.StopAllCoroutines();
                // }

                // Object.DestroyImmediate(obj);
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }

    public void OnGameRestartResume(DateTime startupTime, bool preMonoBehaviourResume)
    {
        if (!_restartOperationStarted || preMonoBehaviourResume) return;

        FinishedRestart = true;
    }
}