using HarmonyLib;
using System;
using UnityEngine;

namespace UniTASPlugin.VersionSafeWrapper;

internal class AsyncOperationWrap
{
    public AsyncOperation instance { get; private set; }

    const string UID_FIELD_NAME = "__UniTAS_UID";

    public AsyncOperationWrap(AsyncOperation asyncOperation)
    {
        // TODO earlier assertion
        WrapperAssertions.AssertInstanceField(typeof(AsyncOperation), UID_FIELD_NAME);
        instance = asyncOperation ?? throw new ArgumentNullException(nameof(asyncOperation));
    }

    static ulong UIDIndex = 1;

    public void AssignUID()
    {
        UID = UIDIndex;
        UIDIndex++;
    }

    public void FinalizeCall()
    {
        // TODO should i manually call or let deconstructor do it?
        Plugin.Log.LogDebug($"FinalizeCall, UID: {UID}");
        if (InstantiatedByUnity)
            return;
        GameTracker.AsyncOperationFinalize(UID);
        UIDIndex--;
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
            uidField.SetValue(value);
        }
    }

    public bool InstantiatedByUnity
    {
        get => UID == 0;
    }
}
