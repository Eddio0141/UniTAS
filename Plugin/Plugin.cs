using System;
using System.Diagnostics;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using StructureMap;
using UniTASPlugin.GameEnvironment;
using UniTASPlugin.GameInfo;
using UniTASPlugin.LegacyGameOverlay;
using UniTASPlugin.LegacySafeWrappers;
using UnityEngine;

namespace UniTASPlugin;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    public static readonly IContainer Kernel = ContainerRegister.Init();

    private static Plugin instance;

    private ManualLogSource _logger;
    public static ManualLogSource Log => instance._logger;

    private PluginWrapper _pluginWrapper;

    private void Awake()
    {
        if (instance != null) return;
        instance = this;
        _logger = Logger;

        var traceCount = Trace.Listeners.Count;
        for (var i = 0; i < traceCount; i++)
        {
            var listener = Trace.Listeners[i];
            if (listener is TraceLogSource) continue;

            Trace.Listeners.RemoveAt(i);
            i--;
            traceCount--;
        }

        _pluginWrapper = Kernel.GetInstance<PluginWrapper>();

        var gameInfo = Kernel.GetInstance<IGameInfo>();
        Logger.LogInfo($"Internally found unity version: {gameInfo.UnityVersion}");
        Logger.LogInfo($"Game product name: {gameInfo.ProductName}");
        Logger.LogDebug($"Mscorlib version: {gameInfo.MscorlibVersion}");
        Logger.LogDebug($"Netstandard version: {gameInfo.NetStandardVersion}");
        // TODO complete fixing this
        var companyNameProperty = Traverse.Create(typeof(Application)).Property("companyName");
        if (companyNameProperty.PropertyExists())
            Logger.LogInfo(
                $"Game company name: {companyNameProperty.GetValue<string>()}"); //product name: {Application.productName}, version: {Application.version}");

        // TODO all axis names for help

        // init random seed
        // TODO make this happen after 
        var env = Kernel.GetInstance<IVirtualEnvironmentFactory>().GetVirtualEnv();
        RandomWrap.InitState((int)env.Seed);

        Logger.LogInfo($"System time: {DateTime.Now}");
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_NAME} is loaded!");
    }

    private void Update()
    {
        _pluginWrapper.Update();
    }

    private void FixedUpdate()
    {
        _pluginWrapper.FixedUpdate();
    }

    private void LateUpdate()
    {
        _pluginWrapper.LateUpdate();
    }

    private void OnGUI()
    {
        Overlay.OnGUI();
    }
}