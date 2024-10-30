using UnityEngine;

namespace UniTAS.Patcher.Services.NoRefresh;

public interface IOverridingCameraInfo
{
    /// <summary>
    /// Called when getting rect
    /// </summary>
    /// <returns>True if being tracked</returns>
    bool GetRect(Camera camera, out Rect rect);
}