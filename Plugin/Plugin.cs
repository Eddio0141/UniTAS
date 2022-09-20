using BepInEx;
using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UniTASPlugin.TAS.Movie;
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

    private void Awake()
    {
        Instance = this;
        Log = Logger;

        UnityVersion = Helper.GetUnityVersion();
        Log.LogInfo($"Internally found version: {UnityVersion}");
        // TODO make this version compatible, v3.5.1 doesn't have those
        //Logger.Log.LogInfo($"Game company name: {Application.companyName}, product name: {Application.productName}, version: {Application.version}");

        Harmony harmony = new($"{NAME}HarmonyPatch");
        harmony.PatchAll();
        
        TAS.Main.AddUnityASyncHandlerID(GetInstanceID());

        // all axis names for help
        // why is this broken TODO
        Log.LogInfo($"All axis names: {string.Join(", ", Input.GetJoystickNames())}");

        // init random seed
        // TODO diff unity versions
        Traverse.Create(typeof(Random)).Method("InitState", new System.Type[] { typeof(int) }).GetValue((int)TAS.Main.Seed());

        TAS.Main.Init();
        TAS.SystemInfo.Init();

        var inputManager = Traverse.CreateWithType("UnityEngine.InputSystem.InputManager");
        var devices = inputManager.Property("devices");
        var devicesList = devices.GetValue();
        var devicesListArray = Traverse.Create(devicesList).Method("ToArray").GetValue<object[]>();
        Log.LogDebug($"Found {devicesListArray.Length} devices: {string.Join(", ", devicesListArray.Select(x => x.ToString()).ToArray())}");

        Log.LogInfo($"System time: {System.DateTime.Now}");
        Log.LogInfo($"Plugin {NAME} is loaded!");
    }

    private void Update()
    {
        GameCapture.Update();
        TAS.Main.Update(Time.deltaTime);

        // TODO remove this test
        if (!TAS.Main.Running && Input.GetKeyDown(KeyCode.K))
        {
            string text = File.ReadAllText("C:\\Program Files (x86)\\Steam\\steamapps\\common\\It Steals\\test.uti");
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
            var base_ = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            Log.LogInfo($"Base: {base_}");
        }
        /*
        if (!TAS.Main.Running && Input.GetKeyDown(KeyCode.L))
        {
            SaveState.Main.Save();
        }
        if (!TAS.Main.Running && Input.GetKeyDown(KeyCode.O))
        {
            SaveState.Main.Load();
        }
        */
    }

    private void FixedUpdate()
    {
        TAS.Main.FixedUpdate();
    }

    public static void AsyncSceneLoad(AsyncOperation operation)
    {
        if (operation == null)
            return;
        if (Instance == null)
        {
            Log.LogWarning("Plugin is null, this should not happen, skipping scene load tracker");
            TAS.Main.LoadingSceneCount = 0;
            return;
        }
        Instance.StartCoroutine(Instance.AsyncSceneLoadWait(operation));
    }

    System.Collections.IEnumerator AsyncSceneLoadWait(AsyncOperation operation)
    {
        // TODO does this work fine
        while (!operation.isDone)
        {
            yield return new WaitForEndOfFrame();
        }
        TAS.Main.LoadingSceneCount--;
    }

    public static void AsyncSceneUnload(AsyncOperation operation)
    {
        if (operation == null)
            return;
        if (Instance == null)
        {
            Log.LogWarning("Plugin is null, this should not happen, skipping scene unload tracker");
            TAS.Main.UnloadingSceneCount = 0;
            return;
        }
        Instance.StartCoroutine(Instance.AsyncSceneUnloadWait(operation));
    }

    System.Collections.IEnumerator AsyncSceneUnloadWait(AsyncOperation operation)
    {
        // TODO does this work fine
        while (!operation.isDone)
        {
            yield return new WaitForEndOfFrame();
        }
        TAS.Main.UnloadingSceneCount--;
    }
}