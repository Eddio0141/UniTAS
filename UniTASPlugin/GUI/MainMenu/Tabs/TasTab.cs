using UnityEngine;

namespace UniTASPlugin.GUI.MainMenu.Tabs;

public class TasTab : IMainMenuTab
{
    public void Render()
    {
        GUILayout.Label("Hello World!");
    }

    public string Name => "TAS";
}