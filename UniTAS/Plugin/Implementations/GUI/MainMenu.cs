using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UniTAS.Plugin.Interfaces.GUI;
using UniTAS.Plugin.Services.EventSubscribers;
using UnityEngine;

namespace UniTAS.Plugin.Implementations.GUI;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class MainMenu : Window
{
    private readonly IMainMenuTab[] _tabs;
    private readonly string[] _tabNames;
    private int _maxTabNameLength;
    private int _currentTab;
    private Vector2 _scrollPosition;

    protected override Rect DefaultWindowRect { get; } = new(0, 0, 600, 200);

    public MainMenu(IUpdateEvents updateEvents, IMainMenuTab[] tabs) : base(updateEvents,
        $"{MyPluginInfo.PLUGIN_NAME} Menu")
    {
        _tabs = tabs;
        _tabNames = tabs.Select(tab => tab.Name).ToArray();
    }

    protected override void OnGUI(int id)
    {
        if (_maxTabNameLength == 0)
        {
            // tab length in pixels
            _maxTabNameLength = _tabNames.Max(name => (int)UnityEngine.GUI.skin.label.CalcSize(new(name)).x) + 20;
        }

        RenderTab(id);
    }

    private void RenderTab(int id)
    {
        GUILayout.BeginHorizontal();
        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.Width(_maxTabNameLength));
        _currentTab = GUILayout.SelectionGrid(_currentTab, _tabNames, 1);
        GUILayout.EndScrollView();

        _tabs[_currentTab].Render(id);
        GUILayout.EndHorizontal();
    }
}