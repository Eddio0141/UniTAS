using System;

namespace UniTASPlugin.UnitySafeWrappers.Interfaces;

public interface IMonoBehaviourWrapper
{
    Type MonoBehaviourType { get; }
    void StopAllCoroutines(object instance);
}