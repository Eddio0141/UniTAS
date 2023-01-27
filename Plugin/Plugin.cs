using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using StructureMap;
using UniTASPlugin.GameInitialRestart;
using UniTASPlugin.Interfaces;
using UniTASPlugin.LegacyGameOverlay;
using UniTASPlugin.Patches.PatchProcessor;
using UnityEngine;

namespace UniTASPlugin;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    public static readonly IContainer Kernel = ContainerRegister.Init();

    private static Plugin instance;

    private ManualLogSource _logger;
    public static ManualLogSource Log => instance._logger;
    public static readonly Harmony Harmony = new($"{MyPluginInfo.PLUGIN_GUID}HarmonyPatch");

    private bool _endOfFrameLoopRunning;

    private bool _initialStartProcessed;
    private List<IPluginInitialLoad> _initialLoadPluginProcessors;

    private IMonoBehEventInvoker _monoBehEventInvoker;
    private bool _fullyLoaded;

    private IGameInitialRestart _gameInitialRestart;

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

        _initialLoadPluginProcessors = Kernel.GetAllInstances<IPluginInitialLoad>().ToList();
        foreach (var processor in _initialLoadPluginProcessors)
        {
            processor.OnInitialLoad();
        }

        var patchProcessors = Kernel.GetAllInstances<OnPluginInitProcessor>();
        var sortedPatches = patchProcessors.SelectMany(x => x.ProcessModules()).OrderByDescending(x => x.Key)
            .Select(x => x.Value);
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
        }

        _monoBehEventInvoker.Update();
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
        if (_fullyLoaded)
        {
            Overlay.OnGUI();
        }
    }

    /// <summary>
    /// This method has to be ran once after all BepInEx patches are loaded
    /// </summary>
    public static void StartEndOfFrameLoop()
    {
        if (instance == null) return;
        if (instance._endOfFrameLoopRunning) return;
        instance._endOfFrameLoopRunning = true;
        instance.StartCoroutine(EndOfFrame());
    }

    private static IEnumerator EndOfFrame()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();
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
        ContainerRegister.ConfigAfterInit(Kernel);

        Kernel.GetInstance<PluginWrapper>();
    }
}