using System;
using BepInEx;
using UniTAS.Plugin.GameEnvironment;
using UniTAS.Plugin.GameRestart;
using UniTAS.Plugin.Logger;
using UniTAS.Plugin.ReverseInvoker;
using Object = UnityEngine.Object;

namespace UniTAS.Plugin.GameInitialRestart;

// ReSharper disable once ClassNeverInstantiated.Global
public class GameInitialRestart : GameRestart.GameRestart, IGameInitialRestart
{
    private readonly ILogger _logger;
    private readonly VirtualEnvironment _virtualEnvironment;

    private bool _restartOperationStarted;
    public bool FinishedRestart { get; private set; }

    private readonly IPatchReverseInvoker _reverseInvoker;

    public GameInitialRestart(RestartParameters restartParameters, IPatchReverseInvoker reverseInvoker) :
        base(restartParameters)
    {
        _reverseInvoker = reverseInvoker;
        _logger = restartParameters.Logger;
        _virtualEnvironment = restartParameters.VirtualEnvironment;
    }

    protected override void OnPreGameRestart()
    {
        if (!_restartOperationStarted) return;

        // TODO fix this hack
        _virtualEnvironment.RunVirtualEnvironment = true;
        _virtualEnvironment.FrameTime = 0.001f;
    }

    protected override void OnGameRestart(bool preSceneLoad)
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

    protected override void OnGameRestartResume(bool preMonoBehaviourResume)
    {
        base.OnGameRestartResume(preMonoBehaviourResume);

        if (!_restartOperationStarted || preMonoBehaviourResume) return;

        FinishedRestart = true;
    }
}