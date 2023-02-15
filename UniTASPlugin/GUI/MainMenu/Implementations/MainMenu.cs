using System.Linq;
using UniTASPlugin.GUI.MainMenu.Tabs;
using UniTASPlugin.Interfaces.Update;
using UnityEngine;

namespace UniTASPlugin.GUI.MainMenu.Implementations;

public partial class MainMenu : IOnGUI
{
    private readonly IMainMenuTab[] _tabs;
    private readonly string[] _tabNames;
    private int _currentTab;

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
        // for tabs to be on the left side of the window
        GUILayout.BeginHorizontal();
        _currentTab = GUILayout.Toolbar(_currentTab, _tabNames);
        GUILayout.EndHorizontal();

        _tabs[_currentTab].Render();
    }
}