using System.Diagnostics.CodeAnalysis;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.MonoBehaviourScripts;
using UnityEngine;

namespace UniTAS.Patcher.Implementations;

[Register]
[ForceInstantiate]
[ExcludeRegisterIfTesting]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class InitManagerGameObject
{
    public const string GameObjectName = "UniTAS Manager";

    public InitManagerGameObject()
    {
        var gameObject = new GameObject(GameObjectName);
        Object.DontDestroyOnLoad(gameObject);

        // attach scripts
        gameObject.AddComponent<MonoBehaviourUpdateInvoker>();
    }
}