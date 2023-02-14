using System.Collections.Generic;
using UnityEngine;

namespace UniTASPlugin.Trackers.DontDestroyOnLoadTracker;

public interface IDontDestroyOnLoadInfo
{
    IEnumerable<Object> DontDestroyOnLoadObjects { get; }
}