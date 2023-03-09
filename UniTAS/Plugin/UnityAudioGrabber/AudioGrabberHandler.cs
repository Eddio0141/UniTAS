using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using UniTAS.Plugin.Interfaces.StartEvent;
using UniTAS.Plugin.Logger;
using UniTAS.Plugin.Utils;
using UnityEngine;

namespace UniTAS.Plugin.UnityAudioGrabber;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class AudioGrabberHandler : IOnAwake, IAudioGrabberInvokes
{
    private readonly IOnAudioFilterRead[] _onAudioFilterReads;
    private readonly ILogger _logger;
    public GameObject GrabberGameObject { get; private set; }

    public AudioGrabberHandler(IOnAudioFilterRead[] onAudioFilterReads, ILogger logger)
    {
        _onAudioFilterReads = onAudioFilterReads;
        _logger = logger;
    }

    public void OnAudioFilterRead(float[] data, int channels)
    {
        foreach (var onAudioFilterRead in _onAudioFilterReads)
        {
            onAudioFilterRead.OnAudioFilterRead(data, channels);
        }
    }

    public void GrabberDisabled()
    {
        Trace.Write("Grabber disabled, attempting to find new one");
        FindNewGrabber();
    }

    public void Awake()
    {
        FindNewGrabber();
    }

    private void FindNewGrabber()
    {
        if (GrabberGameObject != null)
        {
            Trace.Write("Grabber already found, skipping");
            return;
        }
        // needs to be attached on AudioListener

        Trace.Write("Finding AudioListener for grabber");
        GrabberGameObject = UnityObjUtils.FindObjectOfType<AudioListener>()?.gameObject;

        if (GrabberGameObject == null)
        {
            _logger.LogError("AudioListener not found, can't grab audio");
            return;
        }

        Trace.Write("Found AudioListener, adding AudioGrabber");
        GrabberGameObject.AddComponent<AudioGrabber>();

        Trace.Write("AudioGrabber added");
    }
}