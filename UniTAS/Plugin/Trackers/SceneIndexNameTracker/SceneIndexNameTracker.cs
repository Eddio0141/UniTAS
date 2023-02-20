using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using UniTAS.Plugin.Interfaces;
using UniTAS.Plugin.Interfaces.Update;
using UniTAS.Plugin.MonoBehaviourController;
using UniTAS.Plugin.UnitySafeWrappers.Interfaces;

namespace UniTAS.Plugin.Trackers.SceneIndexNameTracker;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class SceneIndexNameTracker : IPluginInitialLoad, ISceneIndexName, IOnUpdate
{
    private readonly List<SceneInfo> _sceneInfos = new();

    private readonly IMonoBehaviourController _monoBehaviourController;
    private readonly ISceneWrapper _sceneWrapper;

    private int? _testingSceneIndex;
    private int _sceneCount;
    private bool _waitingForSceneLoad;

    public SceneIndexNameTracker(IMonoBehaviourController monoBehaviourController, ISceneWrapper sceneWrapper)
    {
        _monoBehaviourController = monoBehaviourController;
        _sceneWrapper = sceneWrapper;
    }

    // disabled for now
    public bool FinishedOperation { get; private set; } = true;

    public void OnInitialLoad()
    {
        // when iterating through all scenes, we must pause execution of mono beh
        _sceneCount = _sceneWrapper.TotalSceneCount;
        // disabled for now
        // TODO paused execution needs to be ran AFTER patching, fix this
        //_monoBehaviourController.PausedExecution = true;
        _testingSceneIndex = null;
        // _testingSceneIndex = 0;
    }

    public void Update()
    {
        if (_testingSceneIndex == null)
        {
            return;
        }

        if (!_waitingForSceneLoad)
        {
            // load scene
            _sceneWrapper.LoadScene(_testingSceneIndex.Value);
            _waitingForSceneLoad = true;
            return;
        }

        // get scene info
        _sceneInfos.Add(new(_testingSceneIndex.Value, _sceneWrapper.ActiveSceneName));

        if (_testingSceneIndex.Value == _sceneCount - 1)
        {
            // done
            foreach (var sceneInfo in _sceneInfos)
            {
                Trace.Write($"Found scene name {sceneInfo.SceneName} with build index {sceneInfo.SceneIndex}");
            }

            _monoBehaviourController.PausedExecution = false;
            _testingSceneIndex = null;
            FinishedOperation = true;
            return;
        }

        // next scene
        _testingSceneIndex++;
        _waitingForSceneLoad = false;
    }

    public int? GetSceneIndex(string sceneName)
    {
        return _sceneInfos.Find(x => x.SceneName == sceneName)?.SceneIndex;
    }

    public string GetSceneName(int sceneIndex)
    {
        return _sceneInfos.Find(x => x.SceneIndex == sceneIndex)?.SceneName;
    }
}