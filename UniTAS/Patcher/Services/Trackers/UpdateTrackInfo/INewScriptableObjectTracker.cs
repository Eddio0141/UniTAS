using UnityEngine;

namespace UniTAS.Patcher.Services.Trackers.UpdateTrackInfo;

public interface INewScriptableObjectTracker
{
    void NewScriptableObject(ScriptableObject scriptableObject);
}