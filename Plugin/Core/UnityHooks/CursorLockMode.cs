using Core.UnityHooks.Helpers;
using System;

namespace Core.UnityHooks;

public class CursorLockMode : BaseEnum<CursorLockMode, CursorLockModeType>
{
    protected override void InitByUnityVersion(Type objType, UnityVersion version)
    {
        switch (version)
        {
            case UnityVersion.v2021_2_14:
                break;
        }
    }
}

public enum CursorLockModeType
{
    None,
    Locked,
    Confined
}