using System.Diagnostics.CodeAnalysis;
using UniTAS.Patcher.Interfaces.Invoker;
using UniTAS.Patcher.MonoBehaviourScripts;
using UnityEngine;

namespace UniTAS.Patcher.Utils;

public static class ManagerGameObject
{
    private static GameObject _gameObject;
    public const string GameObjectName = "UniTAS Manager";

    [InvokeOnPatcherFinish]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static void Init()
    {
        _gameObject = new(GameObjectName);
        Object.DontDestroyOnLoad(_gameObject);

        // attach scripts
        _gameObject.AddComponent<MonoBehaviourUpdateInvoker>();
    }
}