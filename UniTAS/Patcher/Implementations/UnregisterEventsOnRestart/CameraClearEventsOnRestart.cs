using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.UnregisterEventsOnRestart;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.UnregisterEventsOnRestart;

[Singleton]
public class CameraClearEventsOnRestart : EventsClearer
{
    protected override IEnumerable<FieldInfo> FieldsToClear()
    {
        yield return AccessTools.Field(typeof(Camera), "onPreCull");
        yield return AccessTools.Field(typeof(Camera), "onPreRender");
        yield return AccessTools.Field(typeof(Camera), "onPostRender");
    }
}