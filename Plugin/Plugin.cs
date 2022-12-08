using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Ninject;
using Ninject.Modules;
using UniTASPlugin.FakeGameState;
using UniTASPlugin.FakeGameState.GameFileSystem;
using UniTASPlugin.GameEnvironment;
using UniTASPlugin.GameOverlay;
using UniTASPlugin.Movie.ScriptEngine;
using UniTASPlugin.NInjectModules;
using UniTASPlugin.VersionSafeWrapper;
using UnityEngine;
using SystemInfo = UniTASPlugin.FakeGameState.SystemInfo;

// ReSharper disable UnusedMember.Local

namespace UniTASPlugin;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    public static readonly IKernel Kernel = InitKernel();

    private ManualLogSource _logger;

    public int FixedUpdateIndex { get; private set; } = -1;

    public static Plugin Instance;

    public static ManualLogSource Log => Instance._logger;

    private void Awake()
    {
        if (Instance != null)
            return;
        Instance = this;
        _logger = Logger;

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

        // all axis names for help
        // why is this broken TODO
        Logger.LogInfo($"All axis names: {string.Join(", ", Input.GetJoystickNames())}");

        // init random seed
        RandomWrap.InitState((int)GameTime.Seed());

        GameTracker.Init();
        SystemInfo.Init();
        Overlay.Init();

        Logger.LogInfo($"System time: {DateTime.Now}");
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_NAME} is loaded!");
    }

    private static IKernel InitKernel()
    {
        var modules = new INinjectModule[]
        {
            new MovieEngineModule(),
            new GameEnvironmentModule(),
            new PatchReverseInvokerModule()
        };

        return new StandardKernel(modules);
    }

    // unity execution order is Awake() -> FixedUpdate() -> Update()
    private void Update()
    {
        var movieRunner = Kernel.Get<ScriptEngineMovieRunner>();
        movieRunner.Update();
        Overlay.Update();
        // TODO if possible, put this at the first call of Update
        FixedUpdateIndex++;
        GameCapture.Update();
    }

    private void FixedUpdate()
    {
        // TODO if possible, put this at the first call of FixedUpdate
        FixedUpdateIndex = -1;
        // this needs to be called before checking pending soft restart or it will cause a 1 frame desync
        //TAS.FixedUpdate();
        GameRestart.FixedUpdate();
    }

#pragma warning disable IDE0051
    private void LateUpdate()
#pragma warning restore IDE0051
    {
        GameTracker.LateUpdate();
    }

#pragma warning disable IDE0051
    private void OnGUI()
#pragma warning restore IDE0051
    {
        Overlay.OnGUI();
    }
}