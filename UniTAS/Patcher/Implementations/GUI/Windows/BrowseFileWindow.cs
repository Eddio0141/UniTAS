using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.GUI;
using UniTAS.Patcher.Models.GUI;
using UniTAS.Patcher.Services.GUI;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.GUI.Windows;

[Register]
public class BrowseFileWindow : Window, IBrowseFileWindow
{
    private string _path;
    private string[] _files;
    private string[] _paths;

    private const int WIDTH = 600;
    private const int HEIGHT = 200;

    private Vector2 _scroll;

    private readonly Stack<string> _pathPrev = new();
    private readonly Stack<string> _pathNext = new();

    public BrowseFileWindow(WindowDependencies windowDependencies) : base(windowDependencies,
        new(defaultWindowRect: GUIUtils.WindowRect(WIDTH, HEIGHT)))
    {
    }

    public void Open(string title, string path)
    {
        WindowName = title;

        if (!Directory.Exists(path))
        {
            throw new ArgumentException($"Path does not exist: {path}");
        }

        _path = path;
        Show();
    }

    protected override void OnGUI()
    {
        if (_files == null)
        {
            _paths = Directory.GetFileSystemEntries(_path);
            _files = _paths.Select(Path.GetFileName).ToArray();
        }

        GUILayout.BeginHorizontal(GUIUtils.EmptyOptions);

        // back, forward, up
        if (GUILayout.Button("<", GUILayout.ExpandWidth(false)) && _pathPrev.Any())
        {
            _pathNext.Push(_path);
            SetPath(_pathPrev.Pop());
        }

        if (GUILayout.Button(">", GUILayout.ExpandWidth(false)) && _pathNext.Any())
        {
            _pathPrev.Push(_path);
            SetPath(_pathNext.Pop());
        }

        if (GUILayout.Button("^", GUILayout.ExpandWidth(false)))
        {
            var parent = Directory.GetParent(_path)?.FullName;
            if (parent != null)
            {
                _pathPrev.Push(_path);
                _pathNext.Clear();
                SetPath(parent);
            }
        }

        _path = GUILayout.TextField(_path, GUILayout.ExpandWidth(true));

        GUILayout.EndHorizontal();

        GUILayout.BeginVertical(GUIUtils.EmptyOptions);

        _scroll = GUILayout.BeginScrollView(_scroll, false, true);

        for (var i = 0; i < _files.Length; i++)
        {
            var file = _files[i];
            if (!GUILayout.Button(file, GUIUtils.EmptyOptions)) continue;

            // is it file?
            var path = _paths[i];
            if (File.Exists(path))
            {
                OnFileSelected?.Invoke(path);
                Close();
            }
            else
            {
                _pathPrev.Push(_path);
                _pathNext.Clear();
                SetPath(path);
                break;
            }
        }

        GUILayout.EndScrollView();

        GUILayout.EndVertical();
    }

    private void SetPath(string path)
    {
        if (!Directory.Exists(path)) return;
        _path = path;
        _files = null;
        _paths = null;
    }

    public event Action<string> OnFileSelected;
}