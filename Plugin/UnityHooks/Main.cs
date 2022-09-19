using System;
using System.Linq;

namespace UniTASPlugin.UnityHooks;

public static class Main
{
    public static void Init()
    {
        System.Reflection.Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        System.Collections.Generic.IEnumerable<System.Reflection.Assembly> unityCoreModules = assemblies.Where(a => a.GetName().Name == "UnityEngine.CoreModule");

        if (unityCoreModules.Count() == 0)
        {
            Plugin.Log.LogError("Found no UnityEngine.CoreModule assembly, dumping all found assemblies");
            Plugin.Log.LogError(assemblies.Select(a => a.GetName().FullName));
            return;
        }

        System.Reflection.Assembly unityCoreModule = unityCoreModules.ElementAt(0);
        System.Collections.Generic.List<Type> types = unityCoreModule.GetTypes().ToList();

        Type monoBehaviour = null;
        Type @object = null;
        Type scene = null;
        Type sceneManager = null;

        monoBehaviour = types.Find(t => t.FullName == "UnityEngine.MonoBehaviour");
        @object = types.Find(t => t.FullName == "UnityEngine.Object");
        sceneManager = types.Find(t => t.FullName == "UnityEngine.SceneManagement.SceneManager");
        scene = types.Find(t => t.FullName == "UnityEngine.SceneManagement.Scene");

        if (monoBehaviour == null)
            throw new Exception("UnityEngine.MonoBehaviour not found");
        if (@object == null)
            throw new Exception("UnityEngine.Object not found");
        if (scene == null)
            throw new Exception("UnityEngine.SceneManagement.Scene not found");
        if (sceneManager == null)
            throw new Exception("UnityEngine.SceneManagement.SceneManager not found");

        //      /
        new MonoBehavior().Init(monoBehaviour);
        new Object().Init(@object);
        new SceneManager().Init(sceneManager);
        new Scene().Init(scene);
    }
}
