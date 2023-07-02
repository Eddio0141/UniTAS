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

    private string _selectedFile;
    private string _selectedPath;

    private Vector2 _scroll;

    private readonly Stack<string> _pathPrev = new();
    private readonly Stack<string> _pathNext = new();

    public BrowseFileWindow(WindowDependencies windowDependencies, BrowseFileWindowArgs args) : base(windowDependencies,
        new(defaultWindowRect: GUIUtils.WindowRect(Screen.width - 100, Screen.height - 50), windowName: args.Title))
    {
        _path = args.Path;

        if (!Directory.Exists(_path))
        {
            _path = Directory.GetCurrentDirectory();
        }

        Show();
    }

    private readonly GUILayoutOption[] _noExpandWidth = { GUILayout.ExpandWidth(false) };
    private readonly GUILayoutOption[] _expandWidth = { GUILayout.ExpandWidth(true) };

    protected override void OnGUI()
    {
        if (_files == null)
        {
            _paths = Directory.GetFileSystemEntries(_path);
            _files = _paths.Select(Path.GetFileName).ToArray();
        }

        GUILayout.BeginHorizontal(GUIUtils.EmptyOptions);

        // back, forward, up
        if (GUILayout.Button("<", _noExpandWidth) && _pathPrev.Any())
        {
            _pathNext.Push(_path);
            SetPath(_pathPrev.Pop());
        }

        if (GUILayout.Button(">", _noExpandWidth) && _pathNext.Any())
        {
            _pathPrev.Push(_path);
            SetPath(_pathNext.Pop());
        }

        if (GUILayout.Button("^", _noExpandWidth))
        {
            var parent = Directory.GetParent(_path)?.FullName;
            if (parent != null)
            {
                _pathPrev.Push(_path);
                _pathNext.Clear();
                SetPath(parent);
            }
        }

        UnityEngine.GUI.SetNextControlName("PathInput");

        _path = GUILayout.TextField(_path, _expandWidth);

        if (UnityEngine.GUI.GetNameOfFocusedControl() == "PathInput" && Event.current.isKey &&
            Event.current.keyCode == KeyCode.Return && Directory.Exists(_path))
        {
            _pathPrev.Push(_path);
            _pathNext.Clear();
            SetPath(_path);
        }

        GUILayout.EndHorizontal();

        GUILayout.BeginVertical(GUIUtils.EmptyOptions);

        _scroll = GUILayout.BeginScrollView(_scroll, false, true, GUIUtils.EmptyOptions);

        for (var i = 0; i < _files.Length; i++)
        {
            var file = _files[i];
            if (!GUILayout.Button(file, GUIUtils.EmptyOptions)) continue;

            // selection of file
            var path = _paths[i];
            var isFile = File.Exists(path);

            if (isFile)
            {
                _selectedFile = file;
                _selectedPath = path;
            }

            // double click check
            if (Event.current.clickCount != 2) continue;

            // is it file?
            if (isFile)
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
        GUILayout.BeginHorizontal(GUIUtils.EmptyOptions);

        UnityEngine.GUI.SetNextControlName("FileInput");

        _selectedFile = GUILayout.TextField(_selectedFile, _expandWidth);

        // check enter key
        if (UnityEngine.GUI.GetNameOfFocusedControl() == "FileInput" && Event.current.isKey &&
            Event.current.keyCode == KeyCode.Return && File.Exists(_selectedPath))
        {
            OnFileSelected?.Invoke(_selectedPath);
            Close();
        }

        if (GUILayout.Button("Open", _noExpandWidth) && File.Exists(_selectedPath))
        {
            OnFileSelected?.Invoke(_selectedPath);
            Close();
        }

        if (GUILayout.Button("Cancel", _noExpandWidth))
        {
            Close();
        }

        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }

    private void SetPath(string path)
    {
        if (!Directory.Exists(path)) return;
        _path = path;
        _files = null;
        _paths = null;
    }

    protected override void Close()
    {
        base.Close();
        OnClosed?.Invoke();
    }

    public event Action<string> OnFileSelected;

    public event Action OnClosed;
}