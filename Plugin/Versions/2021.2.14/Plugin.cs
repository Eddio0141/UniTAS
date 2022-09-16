﻿using BepInEx;
using Core;
using Core.TAS.Input.Movie;
using HarmonyLib;
using System.IO;
using UnityEngine;

namespace v2021_2_14;

[BepInPlugin(Core.PluginInfo.GUID, Core.PluginInfo.NAME, Core.PluginInfo.VERSION)]
public class Plugin : BaseUnityPlugin
{
#pragma warning disable IDE0051
    private void Awake()
#pragma warning restore IDE0051
    {
        Log.SetLoggers(Logger.LogDebug, Logger.LogError, Logger.LogFatal, Logger.LogInfo, Logger.LogMessage, Logger.LogWarning);

        Log.LogInfo($"Initializing {Core.PluginInfo.NAME}");
        Log.LogInfo($"Game company name: {Application.companyName}, product name: {Application.productName}, version: {Application.version}");
        
        Log.LogDebug($"Unity version: {Application.unityVersion}");
        Core.PluginInfo.Init(Application.unityVersion, typeof(Plugin), typeof(UnityASyncHandler));

        var asyncHandler = new GameObject();
        asyncHandler.AddComponent<UnityASyncHandler>();
        Core.TAS.Main.AddUnityASyncHandlerID(asyncHandler.GetInstanceID());

        var harmony = new Harmony($"{Core.PluginInfo.NAME}HarmonyPatch");
        harmony.PatchAll();

        Log.LogInfo("Initializing unity function helpers");
        InitUnityHelpers();

        Log.LogInfo($"Plugin {Core.PluginInfo.NAME} is loaded!");
    }

    static void InitUnityHelpers()
    {
        //      /InputLegacy
        new Core.UnityHooks.InputLegacy.KeyCode().Init(typeof(KeyCode), UnityVersion.v2021_2_14);
        //      /
        new Core.UnityHooks.Cursor().Init(typeof(Cursor), UnityVersion.v2021_2_14);
        new Core.UnityHooks.CursorLockMode().Init(typeof(CursorLockMode), UnityVersion.v2021_2_14);
        new Core.UnityHooks.MonoBehavior().Init(typeof(MonoBehaviour), UnityVersion.v2021_2_14);
        new Core.UnityHooks.Object().Init(typeof(Object), UnityVersion.v2021_2_14);
        new Core.UnityHooks.SceneManager().Init(typeof(UnityEngine.SceneManagement.SceneManager), UnityVersion.v2021_2_14);
        new Core.UnityHooks.Vector2().Init(typeof(Vector2), UnityVersion.v2021_2_14);
    }

#pragma warning disable IDE0051
    private void Update()
#pragma warning restore IDE0051
    {
        GameCapture.Update();
        Core.TAS.Main.Update(Time.deltaTime);

        // TODO remove this test
        if (!Core.TAS.Main.Running && Input.GetKeyDown(KeyCode.K))
        {
            var text = File.ReadAllText("C:\\Program Files (x86)\\Steam\\steamapps\\common\\It Steals\\test.uti");
            var movie = new Movie("test.uti", text, out var err);

            if (err != "")
            {
                Log.LogError(err);
                return;
            }

            Core.TAS.Main.RunMovie(movie);
        }
    }

#pragma warning disable IDE0051
    private void FixedUpdate()
#pragma warning restore IDE0051
    {
        Core.TAS.Main.FixedUpdate();
    }
}
