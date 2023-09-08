using System;

namespace UniTAS.Patcher.Services;

public interface ISyncFixedUpdateCycle
{
    /// <summary>
    /// Calls the callback on the next fixed update sync
    /// </summary>
    /// <param name="callback">The method to execute on sync</param>
    /// <param name="invokeOffset">Offset of invoking in seconds. Adds number of seconds of offset with the next sync</param>
    /// <param name="fixedUpdateIndex">The FixedUpdate index to sync to, since there could be multiple FixedUpdates in a row, you can choose which one to sync to. The default 0 will sync it to the next Update instead</param>
    void OnSync(Action callback, double invokeOffset = 0, uint fixedUpdateIndex = 0);
}