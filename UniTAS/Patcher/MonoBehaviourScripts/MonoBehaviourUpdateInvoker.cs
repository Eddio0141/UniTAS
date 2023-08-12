using System.Collections;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.MonoBehaviourScripts;

public class MonoBehaviourUpdateInvoker : MonoBehaviour
{
    private IMonoBehEventInvoker _monoBehEventInvoker;

    private void Awake()
    {
        var kernel = ContainerStarter.Kernel;
        _monoBehEventInvoker = kernel.GetInstance<IMonoBehEventInvoker>();
        _monoBehEventInvoker.Awake();
        StartCoroutine(EndOfFrame());
    }

    private void Start()
    {
        _monoBehEventInvoker.Start();
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

    // stupid optimization since object alloc
    private readonly WaitForEndOfFrame _waitForEndOfFrame = new();

    private IEnumerator EndOfFrame()
    {
        while (true)
        {
            yield return _waitForEndOfFrame;
            MonoBehaviourEvents.InvokeLastUpdate();
        }
        // ReSharper disable once IteratorNeverReturns
    }
}