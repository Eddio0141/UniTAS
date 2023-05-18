using System.Collections;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using StructureMap;
using UniTAS.Plugin.Interfaces.Events.MonoBehaviourEvents.DontRunIfPaused;
using UniTAS.Plugin.Interfaces.Events.MonoBehaviourEvents.RunEvenPaused;
using UniTAS.Plugin.Services;
using UniTAS.Plugin.Utils;
using UnityEngine;

namespace UniTAS.Plugin;

[BepInPlugin("dev.yuu0141.unitas.plugin", MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    public static IContainer Kernel;

    private static Plugin _instance;

    private ManualLogSource _logger;
    public static ManualLogSource Log => _instance._logger;
    public static readonly Harmony Harmony = new("dev.yuu0141.unitas.plugin");
    public static ConfigFile PluginConfig => _instance.Config;

    private bool _endOfFrameLoopRunning;

    private IMonoBehEventInvoker _monoBehEventInvoker;
    private IOnLastUpdateUnconditional[] _onLastUpdatesUnconditional;
    private IOnLastUpdateActual[] _onLastUpdatesActual;
    private IMonoBehaviourController _monoBehaviourController;

    private void Awake()
    {
        if (_instance != null) return;
        _logger = Logger;
        _instance = this;
        Kernel = ContainerRegister.Init();

        _logger.LogDebug($"Register info\n{Kernel.WhatDoIHave()}");

        _monoBehEventInvoker = Kernel.GetInstance<IMonoBehEventInvoker>();
        _onLastUpdatesUnconditional = Kernel.GetAllInstances<IOnLastUpdateUnconditional>().ToArray();
        _onLastUpdatesActual = Kernel.GetAllInstances<IOnLastUpdateActual>().ToArray();
        _monoBehaviourController = Kernel.GetInstance<IMonoBehaviourController>();
        Kernel.GetInstance<PluginWrapper>();
    }

    private void Update()
    {
        _monoBehEventInvoker.Update();
    }

    private void FixedUpdate()
    {
        _monoBehEventInvoker.FixedUpdate();
    }

    private void LateUpdate()
    {
        _monoBehEventInvoker.LateUpdate();
    }

    private void OnGUI()
    {
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
            foreach (var update in _onLastUpdatesUnconditional)
            {
                update.OnLastUpdateUnconditional();
            }

            if (_monoBehaviourController.PausedExecution) continue;

            foreach (var update in _onLastUpdatesActual)
            {
                update.OnLastUpdateActual();
            }
        }
        // ReSharper disable once IteratorNeverReturns
    }
}