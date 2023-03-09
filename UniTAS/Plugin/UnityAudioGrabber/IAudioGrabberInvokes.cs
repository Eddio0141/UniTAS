using UnityEngine;

namespace UniTAS.Plugin.UnityAudioGrabber;

/// <summary>
/// Interface for AudioGrabber to invoke so the plugin can grab audio data
/// </summary>
public interface IAudioGrabberInvokes
{
    GameObject GrabberGameObject { get; }
    void OnAudioFilterRead(float[] data, int channels);
    void GrabberDisabled();
}