using UnityEngine;

namespace UniTAS.Patcher.Services.Trackers.UpdateTrackInfo;

public interface IUpdateCameraInfo
{
    /// <summary>
    /// Updates tracking camera info
    /// </summary>
    /// <returns>True if this is being tracked</returns>
    bool SetRect(Camera camera, Rect rect);
}