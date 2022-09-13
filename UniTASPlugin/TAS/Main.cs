using System.Collections.Generic;

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

    static Main()
    {
        // wait for TAS client to open
        // set Running depending on this

        Running = true;
        Time = 0.0;
        axisNames = new List<string>();
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
}
