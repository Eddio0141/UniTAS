using UnityEngine;

namespace UniTAS.Plugin.UnityAudioGrabber;

/// <summary>
/// Script to grab audio data from Unity
/// Attach this script to game object with AudioListener
/// </summary>
public class AudioGrabber : MonoBehaviour
{
    private readonly IAudioGrabberInvokes _audioGrabberInvokes = Plugin.Kernel.GetInstance<IAudioGrabberInvokes>();

    private void OnAudioFilterRead(float[] data, int channels)
    {
        _audioGrabberInvokes.OnAudioFilterRead(data, channels);
    }

    private void OnDisable()
    {
        _audioGrabberInvokes.GrabberDisabled();
    }

    private void OnEnable()
    {
        // in case of duplicate grabbers, destroy the old one
        if (_audioGrabberInvokes.GrabberGameObject != gameObject)
        {
            Destroy(_audioGrabberInvokes.GrabberGameObject);
        }
    }
}