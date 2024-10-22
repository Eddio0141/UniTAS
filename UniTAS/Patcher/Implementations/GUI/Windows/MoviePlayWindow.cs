using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using BepInEx;
using BepInEx.Logging;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.GlobalHotkeyListener;
using UniTAS.Patcher.Interfaces.GUI;
using UniTAS.Patcher.Models.Customization;
using UniTAS.Patcher.Models.GUI;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.Customization;
using UniTAS.Patcher.Services.GUI;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Services.Movie;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.GUI.Windows;

[SuppressMessage("ReSharper", "UnusedType.Global")]
[Singleton]
[ForceInstantiate]
[ExcludeRegisterIfTesting]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class MoviePlayWindow : Window
{
    private string _tasPath = string.Empty;

    private string _tasRunInfo = string.Empty;
    private Vector2 _tasRunInfoScroll;

    private readonly IMovieLogger _movieLogger;
    private readonly IMovieRunner _movieRunner;
    private readonly IBrowseFileWindowFactory _browseFileWindowFileWindowFactory;
    private readonly IConfig _config;
    private IBrowseFileWindow _currentBrowseFileWindow;

    private const string TASPathConfigEntry = "movie-play-window-tas-path";

    public MoviePlayWindow(WindowDependencies windowDependencies, IMovieLogger movieLogger, IMovieRunner movieRunner,
        IBinds binds, IBrowseFileWindowFactory browseFileWindowFileWindowFactory, IGlobalHotkey globalHotkey) :
        base(windowDependencies,
            new(defaultWindowRect: GUIUtils.WindowRect(600, 200), windowName: "Movie Play"), "movieplay")
    {
        _movieLogger = movieLogger;
        _movieRunner = movieRunner;
        _browseFileWindowFileWindowFactory = browseFileWindowFileWindowFactory;
        _config = windowDependencies.Config;
        movieLogger.OnLog += OnMovieLog;

        if (_config.TryGetBackendEntry(TASPathConfigEntry, out string path))
        {
            _tasPath = path;
        }

        var playMovieBind = binds.Create(new("Play movie", KeyCode.Slash, BindCategory.Movie));
        globalHotkey.AddGlobalHotkey(new(playMovieBind, RunMovieWithLogs));
    }

    protected override void OnGUI()
    {
        GUILayout.BeginVertical(GUIUtils.EmptyOptions);
        TASPath();
        OperationButtons();
        TASRunInfo();
        GUILayout.EndVertical();
    }

    private readonly GUILayoutOption[] _moviePathOptions = [GUILayout.ExpandWidth(false)];

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

        if (GUILayout.Button("Browse", GUIUtils.EmptyOptions) && _currentBrowseFileWindow == null)
        {
            _currentBrowseFileWindow = _browseFileWindowFileWindowFactory.Open(new("Browse Movie", Paths.GameRootPath));
            _currentBrowseFileWindow.OnFileSelected += path =>
            {
                _tasPath = path;
                _config.WriteBackendEntry(TASPathConfigEntry, path);
            };
            _currentBrowseFileWindow.OnClosed += () => _currentBrowseFileWindow = null;
        }

        // TODO: implement recent button
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
        [GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true)];

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