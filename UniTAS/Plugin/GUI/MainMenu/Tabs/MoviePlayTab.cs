using System;
using System.IO;
using BepInEx.Logging;
using UniTAS.Plugin.Logger;
using UniTAS.Plugin.Movie;
using UnityEngine;

namespace UniTAS.Plugin.GUI.MainMenu.Tabs;

public class MoviePlayTab : IMainMenuTab
{
    private string _tasPath = string.Empty;

    public string Name => "Movie Play";

    private string _tasRunInfo = string.Empty;
    private Vector2 _tasRunInfoScroll;

    private readonly IMovieLogger _movieLogger;
    private readonly IMovieRunner _movieRunner;

    public MoviePlayTab(IMovieLogger movieLogger, IMovieRunner movieRunner)
    {
        _movieLogger = movieLogger;
        _movieRunner = movieRunner;
        movieLogger.OnLog += OnMovieLog;
    }

    public void Render(int windowID)
    {
        GUILayout.BeginVertical();
        TASPath();
        OperationButtons();
        TASRunInfo();
        GUILayout.EndVertical();
    }

    private void TASPath()
    {
        GUILayout.BeginHorizontal();

        GUILayout.Label("Movie Path", GUILayout.ExpandWidth(false));
        _tasPath = GUILayout.TextField(_tasPath);

        GUILayout.EndHorizontal();
    }

    private void OperationButtons()
    {
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Browse"))
        {
        }

        if (GUILayout.Button("Recent"))
        {
        }

        if (GUILayout.Button("Run"))
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
        catch (Exception e)
        {
            _movieLogger.LogError($"Failed to run TAS movie\n{e}");
        }
    }

    private void TASRunInfo()
    {
        _tasRunInfoScroll = GUILayout.BeginScrollView(_tasRunInfoScroll);

        GUILayout.TextArea(_tasRunInfo, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));

        GUILayout.EndScrollView();
    }

    private void OnMovieLog(object data, LogEventArgs args)
    {
        _tasRunInfo += $"[{args.Level}] {args.Data}{Environment.NewLine}";

        // automatically scroll to bottom
        _tasRunInfoScroll.y = float.MaxValue;
    }
}