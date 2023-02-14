using UnityEngine;

namespace UniTASPlugin.Trackers.DontDestroyOnLoadTracker;

public interface IDontDestroyOnLoadTracker
{
    void DontDestroyOnLoad(Object obj);
}