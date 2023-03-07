using System;

namespace UniTAS.Patcher.Runtime;

public class Events
{
    /// <summary>
    /// Event that is invoked when an audio filter is read
    /// </summary>
    public static event Action<float[], int> OnAudioFilterRead;

    internal static void OnAudioFilterReadInvoke(float[] data, int channels)
    {
        OnAudioFilterRead?.Invoke(data, channels);
    }
}