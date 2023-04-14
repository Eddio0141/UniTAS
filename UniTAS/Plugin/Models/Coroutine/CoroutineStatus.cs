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
            if (value) return;

            OnComplete?.Invoke(this);
            _isRunning = false;
        }
    }

    public Exception Exception { get; set; }
    public event Action<CoroutineStatus> OnComplete;
}