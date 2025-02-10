using System.Collections.Generic;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Services.Trackers.TrackInfo;
using UniTAS.Patcher.Services.Trackers.UpdateTrackInfo;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.Trackers;

[Singleton]
public class DestroyedScriptableObjs : IUpdateScriptableObjDestroyState, IScriptableObjDestroyState
{
    private readonly HashSet<ScriptableObject> _destroyed = new(new HashUtils.ReferenceComparer<ScriptableObject>());

    public void Destroy(ScriptableObject obj) => _destroyed.Add(obj);
    bool IScriptableObjDestroyState.Destroyed(ScriptableObject so) => _destroyed.Contains(so);

    public void ClearState() => _destroyed.Clear();
}