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
}
