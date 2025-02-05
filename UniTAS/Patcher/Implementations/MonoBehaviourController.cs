using System;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Models.DependencyInjection;
using UniTAS.Patcher.Services.GameExecutionControllers;

namespace UniTAS.Patcher.Implementations;

// ReSharper disable once ClassNeverInstantiated.Global
[Singleton(timing: RegisterTiming.Entry)]
public class MonoBehaviourController : IMonoBehaviourController
{
    private bool _pausedExecution;

    public bool PausedExecution
    {
        get => _pausedExecution;
        set
        {
            if (_pausedExecution == value) return;
            _pausedExecution = value;
            OnPauseChange?.Invoke(value);
        }
    }

    public event Action<bool> OnPauseChange;
}