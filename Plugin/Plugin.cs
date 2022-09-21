using BepInEx;
using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using UniTASPlugin.TAS.Movie;
using UniTASPlugin.VersionSafeWrapper;
using UnityEngine;

namespace UniTASPlugin;

[BepInPlugin(GUID, NAME, VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal const string GUID = "UniTASPlugin";
    internal const string NAME = "UniTAS";
    internal const string VERSION = "0.1.0";

    internal static BepInEx.Logging.ManualLogSource Log;

    internal static SemanticVersion UnityVersion;

    internal static Plugin Instance;

    internal static int FixedUpdateIndex { get; private set; } = -1;

    private void Awake()
    {
        Instance = this;
        Log = Logger;

        UnityVersion = Helper.GetUnityVersion();
        Log.LogInfo($"Internally found unity version: {UnityVersion}");
        // TODO complete fixing this
        Log.LogInfo($"Game product name: {AppInfo.ProductName()}");
        //Logger.Log.LogInfo($"Game company name: {Application.companyName}, product name: {Application.productName}, version: {Application.version}");

        Harmony harmony = new($"{NAME}HarmonyPatch");
        harmony.PatchAll();

        // all axis names for help
        // why is this broken TODO
        Log.LogInfo($"All axis names: {string.Join(", ", Input.GetJoystickNames())}");

        // init random seed
        // TODO diff unity versions
        Traverse.Create(typeof(Random)).Method("InitState", new System.Type[] { typeof(int) }).GetValue((int)FakeGameState.GameTime.Seed());

        GameTracker.Init();
        FakeGameState.SystemInfo.Init();

        Log.LogInfo($"System time: {System.DateTime.Now}");
        Log.LogInfo($"Plugin {NAME} is loaded!");
    }

    // unity execution order is Awake() -> FixedUpdate() -> Update()
    private void Update()
    {
        // TODO if possible, put this at the first call of Update
        FixedUpdateIndex++;
        GameCapture.Update();
        TAS.Main.Update(Time.deltaTime);

        // TODO remove this test
        if (!TAS.Main.Running && Input.GetKeyDown(KeyCode.K))
        {
            string text = "";
            if (File.Exists("C:\\Users\\Yuki\\Documents\\test.uti"))
                text = File.ReadAllText("C:\\Users\\Yuki\\Documents\\test.uti");
            else if (File.Exists("C:\\Program Files (x86)\\Steam\\steamapps\\common\\It Steals\\test.uti"))
                text = File.ReadAllText("C:\\Program Files (x86)\\Steam\\steamapps\\common\\It Steals\\test.uti");
            Movie movie = new("test.uti", text, out string err, out List<string> warnings);

            if (err != "")
            {
                Log.LogError(err);
                return;
            }
            if (warnings.Count > 1)
            {
                foreach (string warn in warnings)
                {
                    Log.LogWarning(warn);
                }
            }

            TAS.Main.RunMovie(movie);
        }
        if (!TAS.Main.Running && Input.GetKeyDown(KeyCode.L))
        {
            NewInputSystem.ConnectAllDevices();
        }
        /*
        if (!TAS.Main.Running && Input.GetKeyDown(KeyCode.L))
        {
            SaveState.Main.Save();
        if (!TAS.Main.Running && Input.GetKeyDown(KeyCode.O))
        {
            SaveState.Main.Load();
        }
        */
    }

    private void FixedUpdate()
    {
        // TODO if possible, put this at the first call of FixedUpdate
        FixedUpdateIndex = -1;
        // this needs to be called before checking pending soft restart or it will cause a 1 frame desync
        TAS.Main.FixedUpdate();
        GameRestart.FixedUpdate();
    }
}