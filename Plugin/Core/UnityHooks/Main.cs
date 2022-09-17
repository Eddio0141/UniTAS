using System;
using System.Linq;
using System.Reflection;

namespace Core.UnityHooks;

public static class Main
{
    public static void Init(UnityVersion version, Assembly unityCoreModule)
    {
        var types = unityCoreModule.GetTypes().ToList();

        Type keyCode = null;
        Type cursor = null;
        Type cursorLockMode = null;
        Type monoBehaviour = null;
        Type @object = null;
        Type sceneManager = null;
        Type vector2 = null;
        Type time = null;

        switch (version)
        {
            case UnityVersion.v2018_4_25:
            case UnityVersion.v2021_2_14:
                keyCode = types.Find(t => t.FullName == "UnityEngine.KeyCode");
                cursor = types.Find(t => t.FullName == "UnityEngine.Cursor");
                cursorLockMode = types.Find(t => t.FullName == "UnityEngine.CursorLockMode");
                monoBehaviour = types.Find(t => t.FullName == "UnityEngine.MonoBehaviour");
                @object = types.Find(t => t.FullName == "UnityEngine.Object");
                sceneManager = types.Find(t => t.FullName == "UnityEngine.SceneManagement.SceneManager");
                vector2 = types.Find(t => t.FullName == "UnityEngine.Vector2");
                time = types.Find(t => t.FullName == "UnityEngine.Time");
                break;
        }

        if (keyCode == null)
            throw new Exception("UnityEngine.KeyCode not found");
        if (cursor == null)
            throw new Exception("UnityEngine.Cursor not found");
        if (cursorLockMode == null)
            throw new Exception("UnityEngine.CursorLockMode not found");
        if (monoBehaviour == null)
            throw new Exception("UnityEngine.MonoBehaviour not found");
        if (@object == null)
            throw new Exception("UnityEngine.Object not found");
        if (sceneManager == null)
            throw new Exception("UnityEngine.SceneManagement.SceneManager not found");
        if (vector2 == null)
            throw new Exception("UnityEngine.Vector2 not found");
        if (time == null)
            throw new Exception("UnityEngine.Time not found");

        //      /InputLegacy
        new InputLegacy.KeyCode().Init(keyCode, version);
        //      /
        new Cursor().Init(cursor, version);
        new CursorLockMode().Init(cursorLockMode, version);
        new MonoBehavior().Init(monoBehaviour, version);
        new Object().Init(@object, version);
        new SceneManager().Init(sceneManager, version);
        new Time().Init(time, version);
        new Vector2().Init(vector2, version);
    }
}
