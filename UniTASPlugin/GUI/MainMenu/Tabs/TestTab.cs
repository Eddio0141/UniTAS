using UnityEngine;

namespace UniTASPlugin.GUI.MainMenu.Tabs;

public class TestTab : IMainMenuTab
{
    public void Render(int windowID)
    {
        GUILayout.Label("I am a test tab!");
    }

    public string Name => "Test";
}