using System;
using System.Diagnostics.CodeAnalysis;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.GUI;
using UniTAS.Patcher.Interfaces.TASRenderer;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.EventSubscribers;
using UniTAS.Patcher.Services.GUI;
using UniTAS.Patcher.Services.RuntimeTest;
using UniTAS.Patcher.Services.UnitySafeWrappers.Wrappers;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.GUI.MainMenuTabs;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public class TestTab : IMainMenuTab
{
    private readonly IGameRender _gameRender;
    private readonly IGameRestart _gameRestart;
    private readonly ISceneWrapper _sceneWrapper;
    private readonly IMonoBehaviourController _monoBehaviourController;
    private readonly IRuntimeTestAndLog _runtimeTestAndLog;
    private readonly ILiveScripting _liveScripting;

    public TestTab(IGameRender gameRender, IGameRestart gameRestart, ISceneWrapper sceneWrapper,
        IMonoBehaviourController monoBehaviourController, IRuntimeTestAndLog runtimeTestAndLog,
        ILiveScripting liveScripting)
    {
        _gameRender = gameRender;
        _gameRestart = gameRestart;
        _sceneWrapper = sceneWrapper;
        _monoBehaviourController = monoBehaviourController;
        _runtimeTestAndLog = runtimeTestAndLog;
        _liveScripting = liveScripting;
    }

    public void Render()
    {
        GUILayout.BeginVertical(GUIUtils.EmptyOptions);
        GUILayout.BeginHorizontal(GUIUtils.EmptyOptions);
        if (GUILayout.Button("Start record", GUIUtils.EmptyOptions))
        {
            _gameRender.Start();
        }

        if (GUILayout.Button("Stop record", GUIUtils.EmptyOptions))
        {
            _gameRender.Stop();
        }

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal(GUIUtils.EmptyOptions);
        if (GUILayout.Button("Restart game", GUIUtils.EmptyOptions))
        {
            _gameRestart.SoftRestart(DateTime.Now);
        }

        if (GUILayout.Button("Scene 0", GUIUtils.EmptyOptions))
        {
            _sceneWrapper.LoadScene(0);
        }

        if (GUILayout.Button("Toggle MonoBehaviour pause", GUIUtils.EmptyOptions))
        {
            _monoBehaviourController.PausedExecution = !_monoBehaviourController.PausedExecution;
        }

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal(GUIUtils.EmptyOptions);
        if (GUILayout.Button("Run tests", GUIUtils.EmptyOptions))
        {
            _runtimeTestAndLog.Test();
        }

        if (GUILayout.Button("test scripting", GUIUtils.EmptyOptions))
        {
            _liveScripting.Evaluate("print('hello world')");
        }

        if (GUILayout.Button("spawn window", GUIUtils.EmptyOptions))
        {
            ContainerStarter.Kernel.GetInstance<IWindowFactory>().Create<TestWindow>("test window").Show();
        }

        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }

    [Singleton]
    public class TestWindow : Window
    {
        public TestWindow(IUpdateEvents updateEvents, string windowName = null) : base(updateEvents, windowName)
        {
        }

        protected override Rect DefaultWindowRect => new(50, 50, 200, 200);

        protected override void OnGUI()
        {
        }
    }

    public string Name => "Test";
}