using UnityEngine;

namespace UniTAS.Plugin.Trackers.DontDestroyOnLoadTracker;

public interface IDontDestroyOnLoadTracker
{
    void DontDestroyOnLoad(Object obj);
}