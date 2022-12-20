using System;

namespace UniTASPlugin.UnitySafeWrappers.Interfaces;

public interface IMonoBehaviourWrapper
{
    Type GetMonoBehaviourType();
    void StopAllCoroutines(object instance);
}