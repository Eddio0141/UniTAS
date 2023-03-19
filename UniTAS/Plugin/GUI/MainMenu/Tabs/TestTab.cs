using System.Diagnostics.CodeAnalysis;
using UniTAS.Plugin.GameVideoRender;
using UnityEngine;

namespace UniTAS.Plugin.GUI.MainMenu.Tabs;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public class TestTab : IMainMenuTab
{
    private readonly IGameRender _gameRender;

    public TestTab(IGameRender gameRender)
    {
        _gameRender = gameRender;
    }

    public void Render(int windowID)
    {
        if (GUILayout.Button("Start record"))
        {
            _gameRender.Start();
        }

        if (GUILayout.Button("Stop record"))
        {
            _gameRender.Stop();
        }
    }

    public string Name => "Test";
}