using System.Collections.Generic;

namespace UniTASPlugin.Trackers.DontDestroyOnLoadTracker;

public interface IDontDestroyOnLoadInfo
{
    IEnumerable<object> DontDestroyOnLoadObjects { get; }
}