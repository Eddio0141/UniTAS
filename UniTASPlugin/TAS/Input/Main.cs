using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UniTASPlugin.TAS.Input;

public static class Main
{
    public static Dictionary<string, float> Axis { get; internal set; }

    static Main()
    {
        Axis = new Dictionary<string, float>();
    }

    public static void Update()
    {
        // TODO remove this test
        if (Axis.Count == 0)
        {
            Axis.Add("Mouse X", 0f);
            Axis.Add("Mouse Y", 0f);
        }
        if (TAS.Main.Time < 2)
        {
            Mouse.Position = new Vector2(300, 700);
        }
        else if (TAS.Main.Time < 3)
        {
            Mouse.LeftClick = true;
        }
        else if (TAS.Main.Time < 3.1)
        {
            Mouse.LeftClick = false;
        }
        else if (TAS.Main.Time < 5)
        {
            Axis["Mouse X"] = 1f;
            Axis["Mouse Y"] = 0.2f;
        }
        else if (TAS.Main.Time < 6)
        {
            Axis["Mouse X"] = -2f;
            Axis["Mouse Y"] = 0f;
        }
        else if (TAS.Main.Time > 8 && TAS.Main.Running)
        {
            Plugin.Log.LogDebug("finished");
            TAS.Main.Running = false;
        }

        Mouse.Update();
        VirtualCursor.Update();
    }
}