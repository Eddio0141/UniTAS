using System.Collections;
using UniTAS.Patcher.Services.GameExecutionControllers;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Services.UnityEvents;
using UniTAS.Patcher.Services.UnityInfo;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.MonoBehaviourScripts;

public class MonoBehaviourUpdateInvoker : MonoBehaviour
{
    private IMonoBehEventInvoker _monoBehEventInvoker;
    private ILogger _logger;
    private IGameInfoUpdate _gameInfo;

    private void Awake()
    {
        var kernel = ContainerStarter.Kernel;
        _monoBehEventInvoker = kernel.GetInstance<IMonoBehEventInvoker>();
        _logger = kernel.GetInstance<ILogger>();
        _gameInfo = kernel.GetInstance<IGameInfoUpdate>();

        _monoBehEventInvoker.InvokeAwake();
        
        var controller = kernel.GetInstance<IMonoBehaviourController>();

        var endOfFrame = EndOfFrameCoroutine();
        var fixedUpdate = FixedUpdateCoroutine();
        var update = UpdateCoroutine();
        controller.IgnoreCoroutines.Add(endOfFrame);
        controller.IgnoreCoroutines.Add(fixedUpdate);
        controller.IgnoreCoroutines.Add(update);
        StartCoroutine(endOfFrame);
        StartCoroutine(fixedUpdate);
        StartCoroutine(update);
    }

    private bool _quitting;

    private void OnApplicationQuit()
    {
        _quitting = true;
    }

    private void OnDestroy()
    {
        if (!_quitting)
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

    private void OnApplicationFocus(bool hasFocus)
    {
        _logger.LogDebug($"game in focus = {hasFocus}");
        _gameInfo.IsFocused = hasFocus;
    }

    // stupid optimization since object alloc
    private readonly WaitForEndOfFrame _waitForEndOfFrame = new();
    private readonly WaitForFixedUpdate _waitForFixedUpdate = new();

    private IEnumerator UpdateCoroutine()
    {
        while (true)
        {
            yield return null;
            _monoBehEventInvoker.InvokeUpdate();
        }
        // ReSharper disable once IteratorNeverReturns       
    }

    private IEnumerator EndOfFrameCoroutine()
    {
        while (true)
        {
            yield return _waitForEndOfFrame;
            _monoBehEventInvoker.InvokeLastUpdate();
        }
        // ReSharper disable once IteratorNeverReturns
    }

    private IEnumerator FixedUpdateCoroutine()
    {
        while (true)
        {
            yield return _waitForFixedUpdate;
            _monoBehEventInvoker.InvokeFixedUpdate();
            _monoBehEventInvoker.CoroutineFixedUpdate();
        }
        // ReSharper disable once IteratorNeverReturns
    }
}