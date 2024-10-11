using System;
using HarmonyLib;
using UniTAS.Patcher.Interfaces.UnitySafeWrappers;

namespace UniTAS.Patcher.Implementations.UnitySafeWrappers;

public class FullScreenModeWrap(object instance) : UnityInstanceWrap(instance)
{
    // normal full screen
    public bool FullScreen { get; } = instance == FullScreenModeExclusive || instance == FullScreenModeFullScreen ||
                                      instance == FullScreenModeMaximizedWindow;

    protected override Type WrappedType => StaticWrappedType;
    private static readonly Type StaticWrappedType = AccessTools.TypeByName("UnityEngine.FullScreenMode");

    private static readonly object FullScreenModeExclusive =
        StaticWrappedType == null ? null : Enum.Parse(StaticWrappedType, "ExclusiveFullScreen");

    private static readonly object FullScreenModeFullScreen =
        StaticWrappedType == null ? null : Enum.Parse(StaticWrappedType, "FullScreenWindow");

    private static readonly object FullScreenModeMaximizedWindow =
        StaticWrappedType == null ? null : Enum.Parse(StaticWrappedType, "MaximizedWindow");

    public static readonly object FullScreenModeWindowed =
        StaticWrappedType == null ? null : Enum.Parse(StaticWrappedType, "Windowed");
}