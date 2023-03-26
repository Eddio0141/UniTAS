using System;
using UniTAS.Plugin.Interfaces.DependencyInjection;
using UniTAS.Plugin.Interfaces.Events.SoftRestart;
using UniTAS.Plugin.Services;

namespace UniTAS.Plugin.Implementations.GameRestart;

// ReSharper disable once ClassNeverInstantiated.Global
[Singleton(typeof(GameRestart))]
public class GameInitialRestart : GameRestart, IGameInitialRestart
{
    private bool _restartOperationStarted;
    public bool FinishedRestart { get; private set; }

    private readonly IPatchReverseInvoker _reverseInvoker;

    public GameInitialRestart(RestartParameters restartParameters, IPatchReverseInvoker reverseInvoker) :
        base(restartParameters)
    {
        _reverseInvoker = reverseInvoker;
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