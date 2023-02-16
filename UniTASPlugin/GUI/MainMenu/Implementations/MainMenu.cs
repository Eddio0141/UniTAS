using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UniTASPlugin.GUI.MainMenu.Tabs;
using UniTASPlugin.Interfaces.Update;
using UnityEngine;

namespace UniTASPlugin.GUI.MainMenu.Implementations;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public partial class MainMenu : IOnGUI
{
    private readonly IMainMenuTab[] _tabs;
    private readonly string[] _tabNames;
    private int _currentTab;
    private Vector2 _scrollPosition;

    public MainMenu(IMainMenuTab[] tabs)
    {
        _tabs = tabs;
        _tabNames = tabs.Select(tab => tab.Name).ToArray();
    }

    public void OnGUI()
    {
        RenderBackground();
        RenderTab();
        FinishRenderBackground();
    }

    private void RenderTab()
    {
        GUILayout.BeginHorizontal();
        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.Width(100));
        _currentTab = GUILayout.SelectionGrid(_currentTab, _tabNames, 1);
        GUILayout.EndScrollView();

        _tabs[_currentTab].Render();
        GUILayout.EndHorizontal();
    }
}