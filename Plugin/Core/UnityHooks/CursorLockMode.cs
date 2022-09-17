using Core.UnityHooks.Helpers;
using System;

namespace Core.UnityHooks;

internal class CursorLockMode : BaseEnum<CursorLockMode, CursorLockModeType>
{
    protected override void InitByUnityVersion(Type objType, UnityVersion version) { }
}

internal enum CursorLockModeType
{
    None,
    Locked,
    Confined
}