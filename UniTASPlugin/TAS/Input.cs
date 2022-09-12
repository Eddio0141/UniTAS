using UnityEngine;

namespace UniTASPlugin.TAS;

public static class Input
{
    public static Vector3 mousePosition { get; private set; }

    public static void Update()
    {
        // TODO remove this test
        if (TASTool.Time < 3)
        {
            mousePosition = new Vector3();
        }
        else if (TASTool.Time < 8)
        {
            mousePosition = new Vector3(300, 720);
        }
        else
        {
            TASTool.Running = false;
        }
    }
}
