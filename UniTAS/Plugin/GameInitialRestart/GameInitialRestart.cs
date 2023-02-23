using System;
using UniTAS.Plugin.GameEnvironment;
using UniTAS.Plugin.GameRestart;
using UniTAS.Plugin.ReverseInvoker;

namespace UniTAS.Plugin.GameInitialRestart;

// ReSharper disable once ClassNeverInstantiated.Global
public class GameInitialRestart : GameRestart.GameRestart, IGameInitialRestart
{
    private readonly VirtualEnvironment _virtualEnvironment;

    private bool _restartOperationStarted;
    public bool FinishedRestart { get; private set; }

    private readonly IPatchReverseInvoker _reverseInvoker;

    public GameInitialRestart(RestartParameters restartParameters, IPatchReverseInvoker reverseInvoker,
        VirtualEnvironment virtualEnvironment) :
        base(restartParameters)
    {
        _reverseInvoker = reverseInvoker;
        _virtualEnvironment = virtualEnvironment;
    }

    protected override void OnPreGameRestart()
    {
        base.OnPreGameRestart();

        // TODO fix this hack
        _virtualEnvironment.RunVirtualEnvironment = true;
        _virtualEnvironment.FrameTime = 0.001f;
    }

    protected override void OnGameRestart(bool preSceneLoad)
    {
        base.OnGameRestart(preSceneLoad);

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

    protected override void OnGameRestartResume(bool preMonoBehaviourResume)
    {
        base.OnGameRestartResume(preMonoBehaviourResume);

        if (!_restartOperationStarted || preMonoBehaviourResume) return;

        FinishedRestart = true;
    }
}