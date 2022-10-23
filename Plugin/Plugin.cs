using System;
using BepInEx;
using HarmonyLib;
using Ninject;
using Ninject.Modules;
using UniTASPlugin.FakeGameState.GameFileSystem;
using UniTASPlugin.GameOverlay;
using UniTASPlugin.Movie;
using UniTASPlugin.Movie.ScriptEngine;
using UniTASPlugin.NInjectModules;
using UniTASPlugin.VersionSafeWrapper;
using UnityEngine;
// ReSharper disable UnusedMember.Local

namespace UniTASPlugin;

[BepInPlugin(Guid, Name, Version)]
public class Plugin : BaseUnityPlugin
{
    public const string Guid = "UniTASPlugin";
    public const string Name = "UniTAS";
    public const string Version = "0.1.0";

    public IKernel Kernel = InitKernel();

    public BepInEx.Logging.ManualLogSource Log;

    public int FixedUpdateIndex { get; private set; } = -1;

    public static Plugin Instance;

    private void Awake()
    {
        if (Instance != null)
            return;
        Instance = this;
        Log = Logger;

        Harmony harmony = new($"{Name}HarmonyPatch");
        harmony.PatchAll();

        // init fake file system
        // TODO way of getting device type
        FileSystem.Init(DeviceType.Windows);

        Log.LogInfo($"Internally found unity version: {Helper.GetUnityVersion()}");
        Log.LogInfo($"Game product name: {AppInfo.ProductName()}");
        // TODO complete fixing this
        var companyNameProperty = Traverse.Create(typeof(Application)).Property("companyName");
        if (companyNameProperty.PropertyExists())
            Log.LogInfo($"Game company name: {companyNameProperty.GetValue<string>()}");//product name: {Application.productName}, version: {Application.version}");

        // all axis names for help
        // why is this broken TODO
        Log.LogInfo($"All axis names: {string.Join(", ", Input.GetJoystickNames())}");

        // init random seed
        RandomWrap.InitState((int)FakeGameState.GameTime.Seed());

        GameTracker.Init();
        FakeGameState.SystemInfo.Init();
        Overlay.Init();

        Log.LogInfo($"System time: {System.DateTime.Now}");
        Log.LogInfo($"Plugin {Name} is loaded!");
    }

    private static IKernel InitKernel()
    {
        var modules = new INinjectModule[]
        {
            new MovieModule(),
            new GameEnvironmentModule()
        };

        return new StandardKernel(modules);
    }

    // unity execution order is Awake() -> FixedUpdate() -> Update()
    private void Update()
    {
        var movieRunner = Kernel.Get<MovieRunner<MovieScriptEngine>>();
        var env = Kernel.Get<GameEnvironment.GameEnvironment>();
        movieRunner.Update(ref env);
        Overlay.Update();
        // TODO if possible, put this at the first call of Update
        FixedUpdateIndex++;
        GameCapture.Update();
        throw new NotImplementedException();
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