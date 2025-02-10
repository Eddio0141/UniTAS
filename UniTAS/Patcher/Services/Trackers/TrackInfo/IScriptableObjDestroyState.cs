using UnityEngine;

namespace UniTAS.Patcher.Services.Trackers.TrackInfo;

public interface IScriptableObjDestroyState
{
    bool Destroyed(ScriptableObject so);
}