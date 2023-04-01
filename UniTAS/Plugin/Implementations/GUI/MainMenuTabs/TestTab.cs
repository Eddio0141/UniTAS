using System;
using System.Diagnostics.CodeAnalysis;
using UniTAS.Plugin.Interfaces.GUI;
using UniTAS.Plugin.Interfaces.TASRenderer;
using UniTAS.Plugin.Services;
using UniTAS.Plugin.Services.UnitySafeWrappers.Wrappers;
using UnityEngine;

namespace UniTAS.Plugin.Implementations.GUI.MainMenuTabs;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public class TestTab : IMainMenuTab
{
    private readonly IGameRender _gameRender;
    private readonly IGameRestart _gameRestart;
    private readonly ISceneWrapper _sceneWrapper;
    private readonly IMonoBehaviourController _monoBehaviourController;

    public TestTab(IGameRender gameRender, IGameRestart gameRestart, ISceneWrapper sceneWrapper,
        IMonoBehaviourController monoBehaviourController)
    {
        _gameRender = gameRender;
        _gameRestart = gameRestart;
        _sceneWrapper = sceneWrapper;
        _monoBehaviourController = monoBehaviourController;
    }

    public void Render(int windowID)
    {
        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Start record"))
        {
            _gameRender.Start();
        }

        if (GUILayout.Button("Stop record"))
        {
            _gameRender.Stop();
        }

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Restart game"))
        {
            _gameRestart.SoftRestart(DateTime.Now);
        }

        if (GUILayout.Button("Scene 0"))
        {
            _sceneWrapper.LoadScene(0);
        }

        if (GUILayout.Button("Toggle MonoBehaviour pause"))
        {
            _monoBehaviourController.PausedExecution = !_monoBehaviourController.PausedExecution;
        }

        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }

    public string Name => "Test";
}