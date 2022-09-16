using System.Linq;
using System.Reflection;

namespace Core.UnityHooks;

public static class Main
{
    public static void Init(UnityVersion version, Assembly unityCoreModule)
    {
        var types = unityCoreModule.GetTypes().ToList();

        switch (version)
        {
            case UnityVersion.v2021_2_14:
                var keyCode = types.Find(t => t.FullName == "UnityEngine.KeyCode") ?? throw new System.Exception("UnityEngine.KeyCode not found");
                var cursor = types.Find(t => t.FullName == "UnityEngine.Cursor") ?? throw new System.Exception("UnityEngine.Cursor not found");
                var cursorLockMode = types.Find(t => t.FullName == "UnityEngine.CursorLockMode") ?? throw new System.Exception("UnityEngine.CursorLockMode not found");
                var monoBehaviour = types.Find(t => t.FullName == "UnityEngine.MonoBehaviour") ?? throw new System.Exception("UnityEngine.MonoBehaviour not found");
                var @object = types.Find(t => t.FullName == "UnityEngine.Object") ?? throw new System.Exception("UnityEngine.Object not found");
                var sceneManager = types.Find(t => t.FullName == "UnityEngine.SceneManagement.SceneManager") ?? throw new System.Exception("UnityEngine.SceneManagement.SceneManager not found");
                var vector2 = types.Find(t => t.FullName == "UnityEngine.Vector2") ?? throw new System.Exception("UnityEngine.Vector2 not found");

                //      /InputLegacy
                new InputLegacy.KeyCode().Init(keyCode, version);
                //      /
                new Cursor().Init(cursor, version);
                new CursorLockMode().Init(cursorLockMode, version);
                new MonoBehavior().Init(monoBehaviour, version);
                new Object().Init(@object, version);
                new SceneManager().Init(sceneManager, version);
                new Vector2().Init(vector2, version);
                break;
        }
    }
}
