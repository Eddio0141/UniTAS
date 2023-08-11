using System;

namespace UniTAS.Patcher.Models.Coroutine;

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

            OnCompleteWait?.Invoke(this);
            OnCompleteWait = null;
        }
    }

    public Exception Exception { get; set; }

    private event Action<CoroutineStatus> OnCompleteWait;

    public event Action<CoroutineStatus> OnComplete
    {
        add
        {
            // invoke if its already done
            if (!IsRunning)
            {
                value.Invoke(this);
            }

            OnCompleteWait += value;
        }
        remove => OnCompleteWait -= value;
    }
}