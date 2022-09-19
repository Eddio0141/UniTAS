using System;
using System.Linq;

namespace UniTASPlugin.UnityHooks;

public static class Main
{
    public static void Init()
    {
        Plugin.Log.LogDebug("Calling UnityHooks.Main()");

        System.Reflection.Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        System.Collections.Generic.IEnumerable<System.Reflection.Assembly> unityCoreModules = assemblies.Where(a => a.GetName().Name == "UnityEngine.CoreModule");

        if (unityCoreModules.Count() == 0)
        {
            Plugin.Log.LogError("Found no UnityEngine.CoreModule assembly, dumping all found assemblies");
            Plugin.Log.LogError(assemblies.Select(a => a.GetName().FullName));
            // TODO stop TAS tool from turning into a blackhole
            return;
        }

        System.Reflection.Assembly unityCoreModule = unityCoreModules.ElementAt(0);
        System.Collections.Generic.List<Type> types = unityCoreModule.GetTypes().ToList();

        Type keyCode = null;
        Type cursor = null;
        Type cursorLockMode = null;
        Type monoBehaviour = null;
        Type @object = null;
        Type scene = null;
        Type sceneManager = null;

        keyCode = types.Find(t => t.FullName == "UnityEngine.KeyCode");
        cursor = types.Find(t => t.FullName == "UnityEngine.Cursor");
        cursorLockMode = types.Find(t => t.FullName == "UnityEngine.CursorLockMode");
        monoBehaviour = types.Find(t => t.FullName == "UnityEngine.MonoBehaviour");
        @object = types.Find(t => t.FullName == "UnityEngine.Object");
        sceneManager = types.Find(t => t.FullName == "UnityEngine.SceneManagement.SceneManager");
        scene = types.Find(t => t.FullName == "UnityEngine.SceneManagement.Scene");
        /*
        switch (version)
        {
            case UnityVersion.v2018_4_25:
            case UnityVersion.v2021_2_14:
                break;
        }
        */

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
        if (scene == null)
            throw new Exception("UnityEngine.SceneManagement.Scene not found");
        if (sceneManager == null)
            throw new Exception("UnityEngine.SceneManagement.SceneManager not found");

        //      /
        new Cursor().Init(cursor, Plugin.UnityVersion);
        new CursorLockMode("").Init(cursorLockMode, Plugin.UnityVersion);
        new MonoBehavior().Init(monoBehaviour, Plugin.UnityVersion);
        new Object().Init(@object, Plugin.UnityVersion);
        new SceneManager().Init(sceneManager, Plugin.UnityVersion);
        new Scene().Init(scene, Plugin.UnityVersion);
    }
}
