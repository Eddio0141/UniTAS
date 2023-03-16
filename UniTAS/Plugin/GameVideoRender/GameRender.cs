using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UniTAS.Plugin.Interfaces.Update;
using UniTAS.Plugin.Logger;

namespace UniTAS.Plugin.GameVideoRender;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class GameRender : IGameRender, IOnLastUpdate
{
    private bool _recording;
    private readonly ILogger _logger;

    public int Fps
    {
        get => _videoRenderer.Fps;
        set => _videoRenderer.Fps = value;
    }

    private readonly VideoRenderer _videoRenderer;
    private readonly Renderer[] _renderers;

    public GameRender(ILogger logger, IEnumerable<VideoRenderer> videoRenderers,
        IEnumerable<AudioRenderer> audioRenderers)
    {
        _logger = logger;

        _videoRenderer = videoRenderers.FirstOrDefault(x => x.Available);
        var audioRenderer = audioRenderers.FirstOrDefault(x => x.Available);

        if (_videoRenderer == null)
        {
            _logger.LogError("No video renderer available");
            return;
        }

        if (audioRenderer == null)
        {
            _logger.LogError("No audio renderer available");
            return;
        }

        _renderers = new Renderer[] { _videoRenderer, audioRenderer };
    }

    public void Start()
    {
        if (_recording) return;
        _recording = true;

        _logger.LogInfo("Starting recording");

        foreach (var renderer in _renderers)
        {
            renderer.Start();
        }

        _logger.LogInfo("Started recording");
    }

    public void Stop()
    {
        if (!_recording) return;
        _recording = false;

        _logger.LogInfo("Stopping recording");

        foreach (var renderer in _renderers)
        {
            renderer.Stop();
        }

        _logger.LogInfo("Successfully stopped recording");
    }

    public void OnLastUpdate()
    {
        if (!_recording) return;

        foreach (var renderer in _renderers)
        {
            renderer.Update();
        }
    }
}