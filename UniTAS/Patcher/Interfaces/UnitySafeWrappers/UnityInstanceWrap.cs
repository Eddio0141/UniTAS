using System;
using HarmonyLib;
using UniTAS.Patcher.Services.UnitySafeWrappers;

namespace UniTAS.Patcher.Interfaces.UnitySafeWrappers;

public abstract class UnityInstanceWrap : IUnityInstanceWrap
{
    public object Instance { get; protected set; }
    protected abstract Type WrappedType { get; }

    protected UnityInstanceWrap(object instance)
    {
        Instance = instance;
        Init();
    }

    private void Init()
    {
        if (Instance == null && WrappedType != null)
        {
            Instance = AccessTools.CreateInstance(WrappedType);
        }
    }
}