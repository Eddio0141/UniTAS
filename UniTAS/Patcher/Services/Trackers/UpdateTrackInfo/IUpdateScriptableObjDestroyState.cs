using UnityEngine;

namespace UniTAS.Patcher.Services.Trackers.UpdateTrackInfo;

public interface IUpdateScriptableObjDestroyState
{
    void Destroy(ScriptableObject obj);
    void ClearState();
}