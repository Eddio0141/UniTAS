using System.Collections.Generic;
using UnityEngine;

namespace UniTAS.Patcher.Services.Trackers;

public interface IObjectTracker
{
    void DontDestroyOnLoadAddRoot(Object obj);

    /// <summary>
    /// Contains all DontDestroyOnLoad root game objects.
    /// </summary>
    IEnumerable<Object> DontDestroyOnLoadRootObjects { get; }
}