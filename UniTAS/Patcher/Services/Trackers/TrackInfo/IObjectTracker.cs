using System.Collections.Generic;
using UnityEngine;

namespace UniTAS.Patcher.Services.Trackers.TrackInfo;

public interface IObjectTracker
{
    /// <summary>
    /// Contains all DontDestroyOnLoad root game objects.
    /// </summary>
    IEnumerable<Object> DontDestroyOnLoadRootObjects { get; }
}