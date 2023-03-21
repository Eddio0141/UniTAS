using System.Diagnostics.CodeAnalysis;
using UniTAS.Plugin.Interfaces.GUI;
using UniTAS.Plugin.Interfaces.TASRenderer;
using UnityEngine;

namespace UniTAS.Plugin.Implementations.GUI.MainMenuTabs;

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