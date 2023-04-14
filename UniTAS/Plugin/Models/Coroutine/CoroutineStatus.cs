using System;

namespace UniTAS.Plugin.Models.Coroutine;

public class CoroutineStatus
{
    public bool IsRunning { get; set; } = true;
    public Exception Exception { get; set; }
}