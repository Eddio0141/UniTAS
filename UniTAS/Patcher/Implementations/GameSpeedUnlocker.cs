using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Services.Movie;
using UnityEngine;

namespace UniTAS.Patcher.Implementations;

/// <summary>
/// Removes vsync limit while playing a movie
/// </summary>
[Singleton]
[ForceInstantiate]
public class GameSpeedUnlocker : IGameSpeedUnlocker
{
    public bool Unlock { get; private set; }
    private readonly ILogger _logger;

    public GameSpeedUnlocker(ILogger logger, IMovieRunner movieRunner)
    {
        _logger = logger;
        movieRunner.OnMovieStart += OnMovieStart;
        movieRunner.OnMovieEnd += OnMovieEnd;
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

        _logger.LogDebug(
            $"restored target frame rate to {OriginalTargetFrameRate}, actual value: {Application.targetFrameRate}");
        _logger.LogDebug($"restored vsync count to {OriginalVSyncCount}, actual value: {QualitySettings.vSyncCount}");
    }

    public int OriginalTargetFrameRate { get; set; }
    public int OriginalVSyncCount { get; set; }
}