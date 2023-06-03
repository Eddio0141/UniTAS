using System;

namespace UniTAS.Plugin.Services;

public interface ISyncFixedUpdateCycle
{
    /// <summary>
    /// Calls the callback on the next fixed update sync
    /// </summary>
    /// <param name="callback">The method to execute on sync</param>
    /// <param name="invokeOffset">Offset of invoking in seconds. Adds number of seconds of offset with the next sync</param>
    void OnSync(Action callback, double invokeOffset = 0);
}