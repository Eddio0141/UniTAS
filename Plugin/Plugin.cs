using System;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UniTASFunkyInjector;
using UniTASPlugin.FakeGameState.GameFileSystem;
using UniTASPlugin.GameEnvironment;
using UniTASPlugin.GameOverlay;
using UniTASPlugin.Interfaces.Update;
using UniTASPlugin.Movie;
using UniTASPlugin.Movie.ScriptEngine;
using UniTASPlugin.VersionSafeWrapper;
using UnityEngine;
using SystemInfo = UniTASPlugin.FakeGameState.SystemInfo;

namespace UniTASPlugin;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    public static readonly FunkyInjectorContainer Kernel = ContainerRegister.Init();

    private ManualLogSource _logger;

    private static Plugin instance;

    public static ManualLogSource Log => instance._logger;

    private IOnUpdate[] _onUpdates;
    private IOnFixedUpdate[] _onFixedUpdates;
    private IMovieRunner _movieRunner;

    private void Awake()
    {
        if (instance != null)
            return;
        instance = this;
        _logger = Logger;

        _onUpdates = Kernel.ResolveAll<IOnUpdate>().ToArray();
        _onFixedUpdates = Kernel.ResolveAll<IOnFixedUpdate>().ToArray();
        _movieRunner = Kernel.Resolve<IMovieRunner>();

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
        var env = Kernel.Resolve<IVirtualEnvironmentService>().GetVirtualEnv();
        RandomWrap.InitState((int)env.Seed);

        GameTracker.Init();
        SystemInfo.Init();
        Overlay.Init();

        Logger.LogInfo($"System time: {DateTime.Now}");
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_NAME} is loaded!");
    }

    // unity execution order is Awake() -> FixedUpdate() -> Update()
    private void Update()
    {
        foreach (var update in _onUpdates)
        {
            update.Update(Time.deltaTime);
        }

        _movieRunner.Update();
        //Overlay.Update();
        //GameCapture.Update();
    }

    private void FixedUpdate()
    {
        foreach (var update in _onFixedUpdates)
        {
            update.FixedUpdate(Time.fixedDeltaTime);
        }

        // this needs to be called before checking pending soft restart or it will cause a 1 frame desync
        //TAS.FixedUpdate();
    }

    private void LateUpdate()
    {
        GameTracker.LateUpdate();
    }

    private void OnGUI()
    {
        Overlay.OnGUI();
    }
}