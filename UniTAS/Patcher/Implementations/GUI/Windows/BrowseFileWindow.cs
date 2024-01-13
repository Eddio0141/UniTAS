using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.GUI;
using UniTAS.Patcher.Models.GUI;
using UniTAS.Patcher.Services;
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

    private string _selectedName = string.Empty;
    private string _selectedPath;

    private Vector2 _scroll;

    private readonly Stack<string> _pathPrev = new();
    private readonly Stack<string> _pathNext = new();

    private readonly IPatchReverseInvoker _patchReverseInvoker;

    private readonly string[] _quickAccessPaths =
    [
        Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
        Environment.GetFolderPath(Environment.SpecialFolder.Personal),
        Paths.GameRootPath
    ];

    private readonly string[] _quickAccessNames;

    public BrowseFileWindow(WindowDependencies windowDependencies, BrowseFileWindowArgs args) : base(windowDependencies,
        new(defaultWindowRect: GUIUtils.WindowRect(Screen.width - 200, Screen.height - 200), windowName: args.Title))
    {
        var path = args.Path;

        if (!Directory.Exists(path))
        {
            path = Paths.GameRootPath;
        }

        SetPath(path);

        _quickAccessNames = _quickAccessPaths.Select(Path.GetFileName).ToArray();

        _patchReverseInvoker = windowDependencies.PatchReverseInvoker;

        Show();
    }

    private readonly GUILayoutOption[] _noExpandWidth = [GUILayout.ExpandWidth(false)];
    private readonly GUILayoutOption[] _expandWidth = [GUILayout.ExpandWidth(true)];
    private readonly GUILayoutOption[] _quickAccessButtonWidth = [GUILayout.Width(100)];

    private float _lastFileSelectTime;
    private int _lastFileSelectIndex = -1;
    private const float DOUBLE_CLICK_TIME = 0.3f;

    private float CurrentTime => _patchReverseInvoker.Invoke(() => Time.realtimeSinceStartup);

    protected override void OnGUI()
    {
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

        GUILayout.BeginHorizontal(GUIUtils.EmptyOptions);
        GUILayout.BeginVertical(_quickAccessButtonWidth);

        // quick access
        for (var i = 0; i < _quickAccessPaths.Length; i++)
        {
            var path = _quickAccessPaths[i];
            var name = _quickAccessNames[i];

            if (!GUILayout.Button(name, _quickAccessButtonWidth)) continue;

            _pathPrev.Push(_path);
            _pathNext.Clear();
            SetPath(path);
            break;
        }

        GUILayout.EndVertical();

        // folder view
        _scroll = GUILayout.BeginScrollView(_scroll, false, true, GUIUtils.EmptyOptions);

        for (var i = 0; i < _files.Length; i++)
        {
            var file = _files[i];
            if (!GUILayout.Button(file, _expandWidth)) continue;

            // selection of path
            var path = _paths[i];

            _selectedName = file;
            _selectedPath = path;

            // double click check
            var currentTime = CurrentTime;
            if (_lastFileSelectIndex != i || currentTime > _lastFileSelectTime + DOUBLE_CLICK_TIME)
            {
                _lastFileSelectIndex = i;
                _lastFileSelectTime = currentTime;
                continue;
            }

            // is it file?
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
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal(GUIUtils.EmptyOptions);

        UnityEngine.GUI.SetNextControlName("FileInput");

        _selectedName = GUILayout.TextField(_selectedName, _expandWidth);

        // check enter key
        if (UnityEngine.GUI.GetNameOfFocusedControl() == "FileInput" && Event.current.isKey &&
            Event.current.keyCode == KeyCode.Return)
        {
            TryOpenSelectedFile();

            if (File.Exists(_selectedPath))
            {
                OnFileSelected?.Invoke(_selectedPath);
                Close();
            }
            else
            {
                _selectedPath = string.Empty;
            }
        }

        if (GUILayout.Button("Open", _noExpandWidth))
        {
            TryOpenSelectedFile();
        }

        if (GUILayout.Button("Cancel", _noExpandWidth))
        {
            TryOpenSelectedFile();
        }

        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }

    private void TryOpenSelectedFile()
    {
        _selectedPath = Path.Combine(_path, _selectedName);

        if (File.Exists(_selectedPath))
        {
            OnFileSelected?.Invoke(_selectedPath);
            Close();
            return;
        }

        if (Directory.Exists(_selectedPath))
        {
            _pathPrev.Push(_path);
            _pathNext.Clear();
            SetPath(_selectedPath);
        }

        _selectedName = string.Empty;
        _selectedPath = string.Empty;
    }

    private void SetPath(string path)
    {
        if (!Directory.Exists(path)) return;
        _path = path;
        _paths = Directory.GetFileSystemEntries(_path);
        _files = _paths.Select(Path.GetFileName).ToArray();
    }

    protected override void Close()
    {
        base.Close();
        OnClosed?.Invoke();
    }

    public event Action<string> OnFileSelected;

    public event Action OnClosed;
}