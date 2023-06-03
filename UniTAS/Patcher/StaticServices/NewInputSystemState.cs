using System;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;

namespace UniTAS.Patcher.StaticServices;

public static class NewInputSystemState
{
    public static bool NewInputSystemExists { get; }

    [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
    static NewInputSystemState()
    {
        NewInputSystemExists = false;

        try
        {
            var mouseCurrent = AccessTools.TypeByName("UnityEngine.InputSystem.Mouse");
            var current = AccessTools.Property(mouseCurrent, "current");
            current.GetValue(null, null);

            NewInputSystemExists = true;
        }
        catch (Exception)
        {
            // ignored
        }
    }
}