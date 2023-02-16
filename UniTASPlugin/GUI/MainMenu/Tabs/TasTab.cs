using UnityEngine;

namespace UniTASPlugin.GUI.MainMenu.Tabs;

public class TasTab : IMainMenuTab
{
    public void Render(int windowID)
    {
        GUILayout.Label("Hello World!");
    }

    public string Name => "TAS";
}