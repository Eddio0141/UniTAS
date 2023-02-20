using System;

namespace UniTAS.Plugin.FixedUpdateSync;

public interface ISyncFixedUpdate
{
    /// <summary>
    /// Calls back a method on fixed update sync
    /// </summary>
    /// <param name="callback">Method to call back</param>
    /// <param name="syncOffset">The frame offset from sync frame to call the method backwards, so setting it 1 will call 1 frame before sync happens</param>
    /// <param name="cycleOffset">Number of cycles delayed until execution</param>
    void OnSync(Action callback, uint syncOffset = 0, ulong cycleOffset = 0);
}