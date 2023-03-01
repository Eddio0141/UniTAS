using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace UniTAS.Plugin.GUI.MainMenu.Tabs;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public class TestTab : IMainMenuTab
{
    public void Render(int windowID)
    {
        GUILayout.Label("I am a test tab!");
    }

    public string Name => "Test";
}