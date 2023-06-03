using System.Collections;
using System.Linq;
using HarmonyLib;
using UniTAS.Patcher.Interfaces.Events.MonoBehaviourEvents.DontRunIfPaused;
using UniTAS.Patcher.Interfaces.Events.MonoBehaviourEvents.RunEvenPaused;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.MonoBehaviourScripts;

public class MonoBehaviourUpdateInvoker : MonoBehaviour
{
    public static readonly Harmony Harmony = new("dev.yuu0141.unitas.plugin");

    private IMonoBehEventInvoker _monoBehEventInvoker;
    private IOnLastUpdateUnconditional[] _onLastUpdatesUnconditional;
    private IOnLastUpdateActual[] _onLastUpdatesActual;
    private IMonoBehaviourController _monoBehaviourController;

    private void Awake()
    {
        var kernel = ContainerStarter.Kernel;
        _monoBehEventInvoker = kernel.GetInstance<IMonoBehEventInvoker>();
        _onLastUpdatesUnconditional = kernel.GetAllInstances<IOnLastUpdateUnconditional>().ToArray();
        _onLastUpdatesActual = kernel.GetAllInstances<IOnLastUpdateActual>().ToArray();
        _monoBehaviourController = kernel.GetInstance<IMonoBehaviourController>();

        StartCoroutine(EndOfFrame());
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