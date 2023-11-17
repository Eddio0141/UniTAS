using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.MonoBehaviourScripts;
using UnityEngine;

namespace UniTAS.Patcher.Implementations;

[Register]
[ForceInstantiate]
[ExcludeRegisterIfTesting]
public class InitManagerGameObject
{
    public const string GAME_OBJECT_NAME = "UniTAS Manager";

    public InitManagerGameObject()
    {
        var gameObject = new GameObject(GAME_OBJECT_NAME);
        Object.DontDestroyOnLoad(gameObject);

        // attach scripts
        gameObject.AddComponent<MonoBehaviourUpdateInvoker>();
    }
}