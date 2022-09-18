using System;
using System.Collections.Generic;
using UniTASPlugin.UnityHooks.Helpers;

namespace UniTASPlugin.UnityHooks;

internal class CursorLockMode : BaseEnum<CursorLockMode>
{
    public CursorLockMode(string value) : base(value) { }

    protected override Dictionary<UnityVersion, Type> GetAllEnumTypes()
    {
        return new Dictionary<UnityVersion, Type>()
        {
            {UnityVersion.v2018_4_25, typeof(CursorLockModeType) },
        };
    }

    protected override void InitByUnityVersion(Type objType, UnityVersion version) { }
}

internal enum CursorLockModeType
{
    None,
    Locked,
    Confined
}