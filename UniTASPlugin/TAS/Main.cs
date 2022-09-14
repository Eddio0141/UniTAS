using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace UniTASPlugin.TAS;

public static class Main
{
    static bool _running;
    public static bool Running
    {
        // TODO private set
        get => _running; set
        {
            if (value)
            {
                // TODO set actual framerate
                UnityEngine.Time.captureDeltaTime = 0.001f;
            }
            else
            {
                UnityEngine.Time.captureDeltaTime = 0f;
            }
            _running = value;
        }
    }
    public static double Time { get; private set; }
    static readonly List<string> axisNames;
    static List<int> firstScenes;
    static List<int> firstObjIDs;

    static Main()
    {
        // wait for TAS client to open
        // set Running depending on this

        Running = true;
        Time = 0.0;
        axisNames = new List<string>();

        firstScenes = new List<int>();
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            firstScenes.Add(SceneManager.GetSceneAt(i).buildIndex);
        }
        firstObjIDs = new List<int>();
        foreach (var obj in Object.FindObjectsOfType<MonoBehaviour>())
        {
            firstObjIDs.Add(obj.GetInstanceID());
        }
    }

    public static void Update(float deltaTime)
    {
        if (Running)
        {
            Input.Update();
        }

        Time += deltaTime;
    }

    public static int TimeSeed()
    {
        // TODO: work out seed calculation
        return (int)(Time * 1000.0);
    }

    public static void AxisCall(string axisName)
    {
        if (!axisNames.Contains(axisName))
        {
            axisNames.Add(axisName);

            // notify new found axis
            Plugin.Log.LogInfo($"Found new axis name: {axisName}");
        }
    }

    // BUG fix scene not loading when restarting while loading a scene
    // HACK idea: force captureDeltaTime to be non zero to ensure scene is loaded
    public static void SoftRestart()
    {
        // release mouse lock
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.LoadScene(firstScenes[0]);
        for (int i = 1; i < firstScenes.Count; i++)
        {
            SceneManager.LoadScene(firstScenes[i], LoadSceneMode.Additive);
        }

        // not sure if destroying object after a scene load is good but shouldn't be a problem
        foreach (var obj in Object.FindObjectsOfType<MonoBehaviour>())
        {
            if (!firstObjIDs.Contains(obj.GetInstanceID()))
            {
                if (obj is EventSystem)
                    continue;

                Object.Destroy(obj);
            }
        }

        Plugin.Log.LogInfo("Soft restart");
    }
}
