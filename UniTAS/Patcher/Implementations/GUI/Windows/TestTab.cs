using System;
using System.Diagnostics.CodeAnalysis;
using UniTAS.Patcher.Interfaces.GUI;
using UniTAS.Patcher.Interfaces.TASRenderer;
using UniTAS.Patcher.Models.GUI;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.GameExecutionControllers;
using UniTAS.Patcher.Services.UnitySafeWrappers.Wrappers;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.GUI.Windows;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public class TestTab(
    WindowDependencies windowDependencies,
    IGameRender gameRender,
    IGameRestart gameRestart,
    ISceneManagerWrapper iSceneManagerWrapper,
    IMonoBehaviourController monoBehaviourController)
    : Window(windowDependencies,
        new WindowConfig(windowName: "test", layoutOptions: [GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)]
        ))
{
    protected override void OnGUI()
    {
        GUILayout.BeginVertical(GUIUtils.EmptyOptions);
        GUILayout.BeginHorizontal(GUIUtils.EmptyOptions);
        if (GUILayout.Button("Start record", GUIUtils.EmptyOptions))
        {
            gameRender.Start();
        }

        if (GUILayout.Button("Stop record", GUIUtils.EmptyOptions))
        {
            gameRender.Stop();
        }

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal(GUIUtils.EmptyOptions);
        if (GUILayout.Button("Restart game", GUIUtils.EmptyOptions))
        {
            gameRestart.SoftRestart(DateTime.Now);
        }

        if (GUILayout.Button("Scene 0", GUIUtils.EmptyOptions))
        {
            iSceneManagerWrapper.LoadScene(0);
        }

        if (GUILayout.Button("Toggle MonoBehaviour pause", GUIUtils.EmptyOptions))
        {
            monoBehaviourController.PausedExecution = !monoBehaviourController.PausedExecution;
        }

        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
    }
}