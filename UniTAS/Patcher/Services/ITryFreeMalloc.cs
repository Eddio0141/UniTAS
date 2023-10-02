using System;

namespace UniTAS.Patcher.Services;

public interface ITryFreeMalloc
{
    /// <summary>
    /// Tries to free the memory at the given pointer
    /// </summary>
    void TryFree(IntPtr ptr);
}