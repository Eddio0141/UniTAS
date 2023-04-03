using System.Collections;
using System.Diagnostics;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using StructureMap;
using UniTAS.Plugin.Interfaces.Events.MonoBehaviourEvents.RunEvenPaused;
using UniTAS.Plugin.Services;
using UniTAS.Plugin.Utils;
using UnityEngine;

namespace UniTAS.Plugin;

[BepInPlugin("dev.yuu0141.unitas.plugin", MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    public static readonly IContainer Kernel = ContainerRegister.Init();

    private static Plugin _instance;

    private ManualLogSource _logger;
    public static ManualLogSource Log => _instance._logger;
    public static readonly Harmony Harmony = new("dev.yuu0141.unitas.plugin");
    public static ConfigFile PluginConfig => _instance.Config;

    private bool _endOfFrameLoopRunning;

    private IMonoBehEventInvoker _monoBehEventInvoker;
    private IOnLastUpdateUnconditional[] _onLastUpdates;

    private void Awake()
    {
        if (_instance != null) return;
        _instance = this;
        _logger = Logger;

        Trace.Write(Kernel.WhatDoIHave());

        _monoBehEventInvoker = Kernel.GetInstance<IMonoBehEventInvoker>();
        _onLastUpdates = Kernel.GetAllInstances<IOnLastUpdateUnconditional>().ToArray();
        Kernel.GetInstance<PluginWrapper>();
    }

    private void Update()
    {
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
        // Trace.Write("OnGUI invoke");
        _monoBehEventInvoker.OnGUI();
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
                update.OnLastUpdateUnconditional();
            }
        }
        // ReSharper disable once IteratorNeverReturns
    }
}