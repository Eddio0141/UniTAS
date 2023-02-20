using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using StructureMap;
using UniTAS.Plugin.GameEnvironment;
using UniTAS.Plugin.GameInitialRestart;
using UniTAS.Plugin.Interfaces;
using UniTAS.Plugin.Interfaces.Update;
using UniTAS.Plugin.Patches.PatchProcessor;
using UnityEngine;
using PatchProcessor = UniTAS.Plugin.Patches.PatchProcessor.PatchProcessor;

namespace UniTAS.Plugin;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    public static readonly IContainer Kernel = ContainerRegister.Init();

    private static Plugin _instance;

    private ManualLogSource _logger;
    public static ManualLogSource Log => _instance._logger;
    public static readonly Harmony Harmony = new($"{MyPluginInfo.PLUGIN_GUID}HarmonyPatch");

    private bool _endOfFrameLoopRunning;

    private bool _initialStartProcessed;
    private List<IPluginInitialLoad> _initialLoadPluginProcessors;

    private IMonoBehEventInvoker _monoBehEventInvoker;
    private bool _fullyLoaded;

    private IGameInitialRestart _gameInitialRestart;

    private IOnLastUpdate[] _onLastUpdates;

    private void Awake()
    {
        if (_instance != null) return;
        _instance = this;
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

        Trace.Write(Kernel.WhatDoIHave());

        _initialLoadPluginProcessors = Kernel.GetAllInstances<IPluginInitialLoad>().ToList();
        foreach (var processor in _initialLoadPluginProcessors)
        {
            processor.OnInitialLoad();
        }

        var patchProcessors = Kernel.GetAllInstances<PatchProcessor>();
        var sortedPatches = patchProcessors
            .Where(x => x is OnPluginInitProcessor)
            .SelectMany(x => x.ProcessModules())
            .OrderByDescending(x => x.Key)
            .Select(x => x.Value).ToList();
        Trace.Write($"Patching {sortedPatches.Count} patches on init");
        foreach (var patch in sortedPatches)
        {
            Trace.Write($"Patching {patch.FullName} on init");
            Harmony.PatchAll(patch);
        }

        _monoBehEventInvoker = Kernel.GetInstance<IMonoBehEventInvoker>();
        _gameInitialRestart = Kernel.GetInstance<IGameInitialRestart>();
    }

    private void Update()
    {
        // Trace.Write("Update invoke");
        ProcessInitialStart();
        if (!_fullyLoaded && _gameInitialRestart.FinishedRestart)
        {
            // initial start has finished, load the rest of the plugin
            LoadPluginFull();
            _fullyLoaded = true;
            StartCoroutine(SetInitialFrameTime());
        }

        _monoBehEventInvoker.Update();
    }

    private static IEnumerator SetInitialFrameTime()
    {
        yield return null;
        // TODO fix this hack, from initial game restart code
        Kernel.GetInstance<VirtualEnvironment>().RunVirtualEnvironment = false;
    }

    private void FixedUpdate()
    {
        // Trace.Write($"FixedUpdate, {Time.frameCount}");
        _monoBehEventInvoker.FixedUpdate();
    }

    private void LateUpdate()
    {
        // Trace.Write($"LateUpdate, {Time.frameCount}");
        _monoBehEventInvoker.LateUpdate();
    }

    private void OnGUI()
    {
        // Trace.Write("OnGUI invoke");
        _monoBehEventInvoker.OnGUI();
        
        _windowRect = UnityEngine.GUI.Window(5, _windowRect, DrawWindow, "My Window");
    }
    
    // test imgui stuff
    private Rect _windowRect = new(20, 20, 120, 50);

    private void DrawWindow(int id)
    {
        UnityEngine.GUI.DragWindow();
    }

    /// <summary>
    /// This method has to be ran once after all BepInEx patches are loaded
    /// </summary>
    public static void StartEndOfFrameLoop()
    {
        if (_instance == null) return;
        if (_instance._endOfFrameLoopRunning) return;
        _instance._endOfFrameLoopRunning = true;
        _instance.StartCoroutine(_instance.EndOfFrame());
    }

    private IEnumerator EndOfFrame()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();
            foreach (var update in _onLastUpdates)
            {
                update.OnLastUpdate();
            }
        }
        // ReSharper disable once IteratorNeverReturns
    }

    private void ProcessInitialStart()
    {
        if (_initialStartProcessed) return;
        _initialLoadPluginProcessors.RemoveAll(x => x.FinishedOperation);
        if (_initialLoadPluginProcessors.Count == 0)
        {
            _initialStartProcessed = true;
            _gameInitialRestart.InitialRestart();
        }
    }

    private void LoadPluginFull()
    {
        // ContainerRegister.ConfigAfterInit(Kernel);
        //
        // Trace.Write(Kernel.WhatDoIHave());

        Kernel.GetInstance<PluginWrapper>();
        _onLastUpdates = Kernel.GetAllInstances<IOnLastUpdate>().ToArray();
    }
}