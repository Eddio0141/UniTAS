using System;

namespace Core.UnityHelpers.Types;

public class CursorLockMode : EnumBase
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