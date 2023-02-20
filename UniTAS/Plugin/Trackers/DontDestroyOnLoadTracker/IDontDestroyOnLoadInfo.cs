using System.Collections.Generic;
using UnityEngine;

namespace UniTAS.Plugin.Trackers.DontDestroyOnLoadTracker;

public interface IDontDestroyOnLoadInfo
{
    IEnumerable<Object> DontDestroyOnLoadObjects { get; }
}