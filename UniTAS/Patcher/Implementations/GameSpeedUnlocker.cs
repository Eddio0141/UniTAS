using System.Diagnostics.CodeAnalysis;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Services.Movie;
using UnityEngine;

namespace UniTAS.Patcher.Implementations;

/// <summary>
/// Removes vsync limit while playing a movie
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[Singleton]
public class GameSpeedUnlocker : IGameSpeedUnlocker
{
    public bool Unlock { get; private set; }
    private readonly ILogger _logger;

    public GameSpeedUnlocker(ILogger logger, IMovieRunnerEvents movieEvents)
    {
        _logger = logger;
        movieEvents.OnMovieStart += OnMovieStart;
        movieEvents.OnMovieEnd += OnMovieEnd;
    }

    private void OnMovieStart()
    {
        OriginalTargetFrameRate = Application.targetFrameRate;
        OriginalVSyncCount = QualitySettings.vSyncCount;
        if (Application.targetFrameRate != -1)
        {
            _logger.LogDebug("Unlocking fps");
            Application.targetFrameRate = -1;
            _logger.LogDebug($"Application.targetFrameRate = {Application.targetFrameRate}");
        }

        if (QualitySettings.vSyncCount != 0)
        {
            _logger.LogDebug("Setting vSyncCount to 0");
            QualitySettings.vSyncCount = 0;
            _logger.LogDebug($"QualitySettings.vSyncCount = {QualitySettings.vSyncCount}");
        }

        Unlock = true;
    }

    private void OnMovieEnd()
    {
        Unlock = false;
        Application.targetFrameRate = OriginalTargetFrameRate;
        QualitySettings.vSyncCount = OriginalVSyncCount;
    }

    public int OriginalTargetFrameRate { get; set; }
    public int OriginalVSyncCount { get; set; }
}