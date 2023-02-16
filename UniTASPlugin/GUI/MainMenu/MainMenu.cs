using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UniTASPlugin.GUI.MainMenu.Tabs;
using UniTASPlugin.Interfaces.Update;
using UnityEngine;

namespace UniTASPlugin.GUI.MainMenu;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class MainMenu : IOnGUI
{
    private readonly IMainMenuTab[] _tabs;
    private readonly string[] _tabNames;
    private int _currentTab;
    private Vector2 _scrollPosition;

    private Rect _windowRect = new(0, 0, 600, 200);

    public MainMenu(IMainMenuTab[] tabs)
    {
        _tabs = tabs;
        _tabNames = tabs.Select(tab => tab.Name).ToArray();
    }

    public void OnGUI()
    {
        _windowRect = GUILayout.Window(0, _windowRect, Window, $"{MyPluginInfo.PLUGIN_NAME} Menu");
    }

    private void Window(int id)
    {
        RenderTab(id);

        // make window draggable
        UnityEngine.GUI.DragWindow();
    }

    private void RenderTab(int id)
    {
        GUILayout.BeginHorizontal();
        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.Width(100));
        _currentTab = GUILayout.SelectionGrid(_currentTab, _tabNames, 1);
        GUILayout.EndScrollView();

        _tabs[_currentTab].Render(id);
        GUILayout.EndHorizontal();
    }
}