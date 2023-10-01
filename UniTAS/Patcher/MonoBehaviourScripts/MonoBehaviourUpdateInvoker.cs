using System.Collections;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Services.UnityEvents;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.MonoBehaviourScripts;

public class MonoBehaviourUpdateInvoker : MonoBehaviour
{
    private IMonoBehEventInvoker _monoBehEventInvoker;
    private ILogger _logger;

    private void Awake()
    {
        var kernel = ContainerStarter.Kernel;
        _monoBehEventInvoker = kernel.GetInstance<IMonoBehEventInvoker>();
        _logger = kernel.GetInstance<ILogger>();
        _monoBehEventInvoker.InvokeAwake();
        StartCoroutine(EndOfFrame());
    }

    private void OnDestroy()
    {
        _logger.LogError("MonoBehaviourUpdateInvoker destroyed, this should not happen");
    }

    private void Start()
    {
        _monoBehEventInvoker.InvokeStart();
    }

    private void Update()
    {
        _monoBehEventInvoker.InvokeUpdate();
    }

    private void FixedUpdate()
    {
        _monoBehEventInvoker.InvokeFixedUpdate();
    }

    private void LateUpdate()
    {
        _monoBehEventInvoker.InvokeLateUpdate();
    }

    private void OnGUI()
    {
        _monoBehEventInvoker.InvokeOnGUI();
    }

    private void OnEnable()
    {
        _monoBehEventInvoker.InvokeOnEnable();
    }

    // stupid optimization since object alloc
    private readonly WaitForEndOfFrame _waitForEndOfFrame = new();

    private IEnumerator EndOfFrame()
    {
        while (true)
        {
            yield return _waitForEndOfFrame;
            _monoBehEventInvoker.InvokeLastUpdate();
        }
        // ReSharper disable once IteratorNeverReturns
    }
}