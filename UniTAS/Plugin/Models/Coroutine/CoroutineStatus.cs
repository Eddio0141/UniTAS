using System;

namespace UniTAS.Plugin.Models.Coroutine;

public class CoroutineStatus
{
    private bool _isRunning = true;

    public bool IsRunning
    {
        get => _isRunning;
        set
        {
            if (!_isRunning) return;
            _isRunning = value;

            OnComplete?.Invoke(this);
        }
    }

    public Exception Exception { get; set; }
    public event Action<CoroutineStatus> OnComplete;
}