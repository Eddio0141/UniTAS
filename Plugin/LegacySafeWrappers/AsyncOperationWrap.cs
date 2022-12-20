using System;
using HarmonyLib;
using UnityEngine;

namespace UniTASPlugin.LegacySafeWrappers;

public class AsyncOperationWrap
{
    public AsyncOperation instance { get; private set; }

    private const string UID_FIELD_NAME = "__UniTAS_UID";

    public AsyncOperationWrap(AsyncOperation asyncOperation)
    {
        instance = asyncOperation ?? throw new ArgumentNullException(nameof(asyncOperation));
    }

    private static ulong UIDIndex = 1;

    public void AssignUID()
    {
        UID = UIDIndex;
        if (UIDIndex == ulong.MaxValue)
            UIDIndex = 1;
        else
            UIDIndex++;
    }

    public void FinalizeCall()
    {
        Plugin.Log.LogDebug($"Finalize call async operation, UID: {UID}");
        if (InstantiatedByUnity)
            return;
        GameTracker.AsyncOperationFinalize(UID);
        AssetBundleCreateRequestWrap.FinalizeCall(UID);
        AssetBundleRequestWrap.FinalizeCall(UID);
    }

    public ulong UID
    {
        get
        {
            var uidField = Traverse.Create(instance).Field(UID_FIELD_NAME);
            return uidField.GetValue<ulong>();
        }
        set
        {
            var uidField = Traverse.Create(instance).Field(UID_FIELD_NAME);
            _ = uidField.SetValue(value);
        }
    }

    public bool InstantiatedByUnity => UID == 0;
}