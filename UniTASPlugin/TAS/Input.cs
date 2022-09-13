using System.Runtime.InteropServices;
using UnityEngine;

namespace UniTASPlugin.TAS;

public static class Input
{
    public static Vector3 mousePosition { get; private set; }
    public static bool mousePresent { get; private set; }

    static Input()
    {
        mousePosition = Vector3.zero;
        mousePresent = true;
    }

    public static void Update()
    {
        // TODO remove this test
        if (TASTool.Time < 10)
        {
            TASTool.Running = true;
            var x = TASTool.Time / 10 * 1920;
            var y = TASTool.Time % 1 * 1080;

            mousePosition = new Vector3((float)x, (float)y);
        }
        else
        {
            TASTool.Running = false;
        }
    }
}
