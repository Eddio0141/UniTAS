using Core.UnityHook.Helpers;
using System;

namespace Core.UnityHook;

public class CursorLockMode : BaseEnum
{
    protected override void InitByUnityVersion(Type objType, UnityVersion version)
    {
        switch (version)
        {
            case UnityVersion.v2021_2_14:
                break;
        }
    }

    protected override Type GetEnumType()
    {
        return typeof(CursorLockModeType);
    }
}

internal enum CursorLockModeType
{
    None,
    Locked,
    Confined
}