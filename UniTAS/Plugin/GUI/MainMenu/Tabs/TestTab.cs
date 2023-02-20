using UnityEngine;

namespace UniTAS.Plugin.GUI.MainMenu.Tabs;

public class TestTab : IMainMenuTab
{
    public void Render(int windowID)
    {
        GUILayout.Label("I am a test tab!");
    }

    public string Name => "Test";
}