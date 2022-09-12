namespace UniTASPlugin;

public class TAS
{
    public static TAS Instance;

    public bool Running { get; set; }

    public TAS()
    {
        Running = false;

        Instance = this;
    }
}
