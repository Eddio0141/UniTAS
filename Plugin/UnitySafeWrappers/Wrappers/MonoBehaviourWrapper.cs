using System;
using UniTASPlugin.UnitySafeWrappers.Interfaces;
using UnityEngine;

namespace UniTASPlugin.UnitySafeWrappers.Wrappers;

public class MonoBehaviourWrapper : IMonoBehaviourWrapper
{
    public Type GetMonoBehaviourType()
    {
        return typeof(MonoBehaviour);
    }

    public void StopAllCoroutines(object instance)
    {
        ((MonoBehaviour)instance).StopAllCoroutines();
    }
}