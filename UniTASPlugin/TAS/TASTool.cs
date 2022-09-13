namespace UniTASPlugin.TAS;

public static class TASTool
{
    private static bool _running;
    public static bool Running
    {
        // TODO private set
        get => _running; set
        {
            if (value)
            {
                // TODO set actual framerate
                UnityEngine.Time.captureFramerate = 1000;
            }
            else
            {
                UnityEngine.Time.captureFramerate = 0;
            }
            _running = value;
        }
    }
    public static double Time { get; private set; }

    static TASTool()
    {
        // wait for TAS client to open
        // set Running depending on this

        Running = true;
        Time = 0.0;
    }

    public static void Update(float deltaTime)
    {
        Time += deltaTime;

        Input.Update();

        if (!Running)
            return;
    }

    public static int TimeSeed()
    {
        // TODO: work out seed calculation
        return (int)(Time * 1000.0);
    }
}
