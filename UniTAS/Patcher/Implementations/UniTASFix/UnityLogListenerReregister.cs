using System;
using System.Reflection;
using BepInEx.Logging;
using HarmonyLib;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.SoftRestart;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.UniTASFix;

[Singleton]
// this is to get around the registered loggers being erased by game restart
public class UnityLogListenerReregister : IOnGameRestart
{
    public void OnGameRestart(DateTime startupTime, bool preSceneLoad)
    {
        if (preSceneLoad) return;

        // section from UnityLogSource's static constructor
        var onUnityLogMessageReceived = AccessTools.DeclaredMethod(typeof(UnityLogSource), "OnUnityLogMessageReceived",
            [typeof(string), typeof(string), typeof(LogType)]);
        var handler = Delegate.CreateDelegate(typeof(Application.LogCallback), onUnityLogMessageReceived);
        var eventInfo = typeof(Application).GetEvent("logMessageReceived", BindingFlags.Static | BindingFlags.Public);
        if (eventInfo != null)
            eventInfo.AddEventHandler(null, handler);
        else
            typeof(Application).GetMethod("RegisterLogCallback", BindingFlags.Static | BindingFlags.Public)!.Invoke(
                null, [
                    handler
                ]);
    }
}