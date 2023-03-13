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
        get => _gameVideoRenderer.Fps;
        set => _gameVideoRenderer.Fps = value;
    }

    private readonly Renderer[] _renderers;
    private readonly GameVideoRenderer _gameVideoRenderer;

    public GameRender(ILogger logger, IEnumerable<Renderer> renderers)
    {
        _logger = logger;

        _renderers = renderers.Where(x => x.Available).ToArray();
        _gameVideoRenderer = (GameVideoRenderer)_renderers.FirstOrDefault(x => x is GameVideoRenderer);
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