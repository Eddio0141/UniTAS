using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using UniTAS.Plugin.Logger;
using UniTAS.Plugin.Movie.RunnerEvents;
using UnityEngine;

namespace UniTAS.Plugin.GameSpeedUnlocker;

/// <summary>
/// Removes vsync limit while playing a movie
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class GameSpeedUnlocker : IGameSpeedUnlocker, IOnMovieStart, IOnMovieEnd
{
    public bool Unlock { get; private set; }
    private readonly ILogger _logger;

    public GameSpeedUnlocker(ILogger logger)
    {
        _logger = logger;
    }

    public void OnMovieStart()
    {
        OriginalTargetFrameRate = Application.targetFrameRate;
        OriginalVSyncCount = QualitySettings.vSyncCount;
        if (Application.targetFrameRate != -1)
        {
            _logger.LogDebug("Unlocking fps");
            Application.targetFrameRate = -1;
            Trace.Write($"Application.targetFrameRate = {Application.targetFrameRate}");
        }

        if (QualitySettings.vSyncCount != 0)
        {
            _logger.LogDebug("Setting vSyncCount to 0");
            QualitySettings.vSyncCount = 0;
            Trace.Write($"QualitySettings.vSyncCount = {QualitySettings.vSyncCount}");
        }

        Unlock = true;
    }

    public void OnMovieEnd()
    {
        Unlock = false;
        Application.targetFrameRate = OriginalTargetFrameRate;
        QualitySettings.vSyncCount = OriginalVSyncCount;
    }

    public int OriginalTargetFrameRate { get; set; }
    public int OriginalVSyncCount { get; set; }
}