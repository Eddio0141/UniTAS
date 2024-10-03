using UnityEngine;

namespace UniTAS.Patcher.Services.Trackers.UpdateTrackInfo;

public interface IObjectTrackerUpdate
{
    void DontDestroyOnLoadAddRoot(Object obj);
}