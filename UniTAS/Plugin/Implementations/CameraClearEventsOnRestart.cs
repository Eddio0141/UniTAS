using System.Reflection;
using HarmonyLib;
using UniTAS.Plugin.Interfaces.DependencyInjection;
using UniTAS.Plugin.Interfaces.Events.SoftRestart;
using UnityEngine;

namespace UniTAS.Plugin.Implementations;

[Singleton]
public class CameraClearEventsOnRestart : IOnPreGameRestart
{
    private readonly FieldInfo _onPreCullEventField = AccessTools.Field(typeof(Camera), "onPreCull");
    private readonly FieldInfo _onPreRenderEventField = AccessTools.Field(typeof(Camera), "onPreRender");
    private readonly FieldInfo _onPostRenderEventField = AccessTools.Field(typeof(Camera), "onPostRender");

    public void OnPreGameRestart()
    {
        _onPreCullEventField.SetValue(null, null);
        _onPreRenderEventField.SetValue(null, null);
        _onPostRenderEventField.SetValue(null, null);
    }
}