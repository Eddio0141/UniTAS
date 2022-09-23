using HarmonyLib;
using System;
using UnityEngine;

namespace UniTASPlugin.VersionSafeWrapper;

internal class AsyncOperationWrap
{
    public AsyncOperation instance { get; private set; }

    const string M_PTR_FIELD_NAME = "m_Ptr";

    public AsyncOperationWrap(AsyncOperation asyncOperation)
    {
        // TODO earlier assertion
        WrapperAssertions.AssertInstanceField(typeof(AsyncOperation), M_PTR_FIELD_NAME);
        instance = asyncOperation ?? throw new ArgumentNullException(nameof(asyncOperation));
    }

    public IntPtr m_Ptr
    {
        get
        {
            var m_ptrField = Traverse.Create(instance).Field(M_PTR_FIELD_NAME);
            return m_ptrField.GetValue<IntPtr>();
        }
    }
}
