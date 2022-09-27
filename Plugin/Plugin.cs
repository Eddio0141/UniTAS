using BepInEx;
using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using UniTASPlugin.TASMovie;
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

    internal static int FixedUpdateIndex { get; private set; } = -1;

    void Awake()
    {
        Log = Logger;

        UnityVersion = Helper.GetUnityVersion();
        Log.LogInfo($"Internally found unity version: {UnityVersion}");
        Log.LogInfo($"Game product name: {AppInfo.ProductName()}");
        // TODO complete fixing this
        var companyNameProperty = Traverse.Create(typeof(Application)).Property("companyName");
        if (companyNameProperty.PropertyExists())
            Log.LogInfo($"Game company name: {companyNameProperty.GetValue<string>()}");//product name: {Application.productName}, version: {Application.version}");

        Harmony harmony = new($"{NAME}HarmonyPatch");
        harmony.PatchAll();

        // all axis names for help
        // why is this broken TODO
        Log.LogInfo($"All axis names: {string.Join(", ", Input.GetJoystickNames())}");

        // init random seed
        RandomWrap.InitState((int)FakeGameState.GameTime.Seed());

        GameTracker.Init();
        FakeGameState.SystemInfo.Init();
        Overlay.Init();

        Log.LogInfo($"System time: {System.DateTime.Now}");
        Log.LogInfo($"Plugin {NAME} is loaded!");
    }

    // unity execution order is Awake() -> FixedUpdate() -> Update()
    void Update()
    {
        // TODO if possible, put this at the first call of Update
        FixedUpdateIndex++;
        GameCapture.Update();
        TAS.Update();

        // TODO remove this test
        if (!TAS.Running && Input.GetKeyDown(KeyCode.K))
        {
            string text = "";
            if (File.Exists("C:\\Users\\Yuki\\Documents\\test.uti"))
                text = File.ReadAllText("C:\\Users\\Yuki\\Documents\\test.uti");
            else if (File.Exists("C:\\Program Files (x86)\\Steam\\steamapps\\common\\It Steals\\test.uti"))
                text = File.ReadAllText("C:\\Program Files (x86)\\Steam\\steamapps\\common\\It Steals\\test.uti");
            var movie = new Movie("test.uti", text, out string err, out List<string> warnings);

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

            TAS.RunMovie(movie);
        }
        if (!TAS.Running && Input.GetKeyDown(KeyCode.L))
        {
            GameRestart.SoftRestart(new System.DateTime());
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

    void FixedUpdate()
    {
        // TODO if possible, put this at the first call of FixedUpdate
        FixedUpdateIndex = -1;
        // this needs to be called before checking pending soft restart or it will cause a 1 frame desync
        TAS.FixedUpdate();
        GameRestart.FixedUpdate();
    }

    void LateUpdate()
    {
        GameTracker.LateUpdate();
    }

    void OnGUI()
    {
        Overlay.Update();
    }
}