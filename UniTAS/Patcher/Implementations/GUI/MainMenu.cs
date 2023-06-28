using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.GUI;
using UniTAS.Patcher.Models.GUI;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.GUI;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[Singleton]
[ForceInstantiate]
public class MainMenu : Window
{
    private readonly IMainMenuTab[] _tabs;
    private readonly string[] _tabNames;
    private int _maxTabNameLength;
    private int _currentTab;
    private Vector2 _scrollPosition;

    public MainMenu(WindowDependencies windowDependencies, IMainMenuTab[] tabs) :
        base(windowDependencies, new(windowName: "UniTAS Menu", defaultWindowRect: new(20, 20, 600, 200)))
    {
        _tabs = tabs;
        _tabNames = tabs.Select(tab => tab.Name).ToArray();
        Show();
    }

    protected override void OnGUI()
    {
        if (_maxTabNameLength == 0)
        {
            // tab length in pixels
            _maxTabNameLength = _tabNames.Max(name => (int)UnityEngine.GUI.skin.label.CalcSize(new(name)).x) + 20;
        }

        RenderTab();
    }

    private GUILayoutOption[] _scrollPositionOptions;

    private void RenderTab()
    {
        GUILayout.BeginHorizontal(GUIUtils.EmptyOptions);
        _scrollPositionOptions ??= new[] { GUILayout.Width(_maxTabNameLength) };
        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, _scrollPositionOptions);
        _currentTab = GUILayout.SelectionGrid(_currentTab, _tabNames, 1, GUIUtils.EmptyOptions);
        GUILayout.EndScrollView();

        _tabs[_currentTab].Render();
        GUILayout.EndHorizontal();
    }
}