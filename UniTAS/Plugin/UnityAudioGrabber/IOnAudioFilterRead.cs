namespace UniTAS.Plugin.UnityAudioGrabber;

public interface IOnAudioFilterRead
{
    void OnAudioFilterRead(float[] data, int channels);
}