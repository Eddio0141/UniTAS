using Core.UnityHooks.Helpers;
using System;

namespace Core.UnityHooks;

internal class CursorLockMode : BaseEnum<CursorLockMode, CursorLockModeType>
{
    protected override void InitByUnityVersion(Type objType, UnityVersion version)
    {
        switch (version)
        {
            case UnityVersion.v2021_2_14:
                break;
            case UnityVersion.v2018_4_25:
                break;
        }
    }
}

internal enum CursorLockModeType
{
    None,
    Locked,
    Confined
}