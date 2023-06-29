using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using BepInEx.Logging;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.GUI;
using UniTAS.Patcher.Models.Customization;
using UniTAS.Patcher.Models.GUI;
using UniTAS.Patcher.Services.Customization;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Services.Movie;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.GUI.Windows;

[SuppressMessage("ReSharper", "UnusedType.Global")]
[Singleton]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class MoviePlayWindow : Window
{
    private string _tasPath = string.Empty;

    private string _tasRunInfo = string.Empty;
    private Vector2 _tasRunInfoScroll;

    private readonly IMovieLogger _movieLogger;
    private readonly IMovieRunner _movieRunner;

    private readonly Bind _playMovieBind;

    public MoviePlayWindow(WindowDependencies windowDependencies, IMovieLogger movieLogger, IMovieRunner movieRunner,
        IBinds binds) :
        base(windowDependencies,
            new(defaultWindowRect: GUIUtils.WindowRect(600, 200), windowName: "Movie Play"))
    {
        _movieLogger = movieLogger;
        _movieRunner = movieRunner;
        movieLogger.OnLog += OnMovieLog;

        _playMovieBind = binds.Create(new("PlayMovie", KeyCode.Slash));
        windowDependencies.UpdateEvents.OnUpdateUnconditional += UpdateUnconditional;
    }

    protected override void OnGUI()
    {
        GUILayout.BeginVertical(GUIUtils.EmptyOptions);
        TASPath();
        OperationButtons();
        TASRunInfo();
        GUILayout.EndVertical();
    }

    public void UpdateUnconditional()
    {
        if (_playMovieBind.IsPressed())
        {
            RunMovieWithLogs();
        }
    }

    private readonly GUILayoutOption[] _moviePathOptions = { GUILayout.ExpandWidth(false) };

    private void TASPath()
    {
        GUILayout.BeginHorizontal(GUIUtils.EmptyOptions);

        GUILayout.Label("Movie Path", _moviePathOptions);
        _tasPath = GUILayout.TextField(_tasPath, GUIUtils.EmptyOptions);

        GUILayout.EndHorizontal();
    }

    private void OperationButtons()
    {
        GUILayout.BeginHorizontal(GUIUtils.EmptyOptions);

        // TODO: implement browse and recent buttons
        // if (GUILayout.Button("Browse"))
        // {
        // }
        //
        // if (GUILayout.Button("Recent"))
        // {
        // }

        if (GUILayout.Button("Run", GUIUtils.EmptyOptions))
        {
            RunMovieWithLogs();
        }

        GUILayout.EndHorizontal();
    }

    private void RunMovieWithLogs()
    {
        // clear log at movie run
        _tasRunInfo = string.Empty;

        // initial checks
        if (!File.Exists(_tasPath))
        {
            _movieLogger.LogError("TAS movie file does not exist!");
            return;
        }

        _movieLogger.LogInfo("Starting TAS movie...");

        // run movie
        try
        {
            var movie = File.ReadAllText(_tasPath);
            _movieRunner.RunFromInput(movie);
        }
        catch (IOException e)
        {
            _movieLogger.LogError($"Failed to read TAS movie file\n{e}");
        }
    }

    private readonly GUILayoutOption[] _tasRunInfoOptions =
        { GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true) };

    private void TASRunInfo()
    {
        _tasRunInfoScroll = GUILayout.BeginScrollView(_tasRunInfoScroll, GUIUtils.EmptyOptions);

        GUILayout.TextArea(_tasRunInfo, _tasRunInfoOptions);

        GUILayout.EndScrollView();
    }

    private void OnMovieLog(object data, LogEventArgs args)
    {
        _tasRunInfo += $"[{args.Level}] {args.Data}{Environment.NewLine}";

        // automatically scroll to bottom
        _tasRunInfoScroll.y = float.MaxValue;
    }
}