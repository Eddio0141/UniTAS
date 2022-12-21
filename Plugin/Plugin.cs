using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using StructureMap;
using UniTASPlugin.GameEnvironment;
using UniTASPlugin.LegacyFakeGameState.GameFileSystem;
using UniTASPlugin.LegacyGameOverlay;
using UniTASPlugin.LegacySafeWrappers;
using UnityEngine;
using SystemInfo = UniTASPlugin.LegacyFakeGameState.SystemInfo;

namespace UniTASPlugin;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    public static readonly IContainer Kernel = ContainerRegister.Init();

    private ManualLogSource _logger;

    private static Plugin instance;

    public static ManualLogSource Log => instance._logger;

    private PluginWrapper _pluginWrapper;

    private void Awake()
    {
        if (instance != null)
            return;
        instance = this;
        _logger = Logger;

        _pluginWrapper = Kernel.GetInstance<PluginWrapper>();

        Logger.LogInfo("init patch");
        Harmony harmony = new($"{MyPluginInfo.PLUGIN_GUID}HarmonyPatch");
        harmony.PatchAll();
        Logger.LogInfo("post init patch");

        // init fake file system
        // TODO way of getting device type
        FileSystem.Init(DeviceType.Windows);

        Logger.LogInfo($"Internally found unity version: {Helper.GetUnityVersion()}");
        Logger.LogInfo($"Game product name: {AppInfo.ProductName()}");
        // TODO complete fixing this
        var companyNameProperty = Traverse.Create(typeof(Application)).Property("companyName");
        if (companyNameProperty.PropertyExists())
            Logger.LogInfo(
                $"Game company name: {companyNameProperty.GetValue<string>()}"); //product name: {Application.productName}, version: {Application.version}");

        // TODO all axis names for help

        // init random seed
        var env = Kernel.GetInstance<IVirtualEnvironmentFactory>().GetVirtualEnv();
        RandomWrap.InitState((int)env.Seed);

        GameTracker.Init();
        SystemInfo.Init();
        Overlay.Init();

        Logger.LogInfo($"System time: {DateTime.Now}");
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_NAME} is loaded!");
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