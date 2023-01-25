using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UniTASPlugin.Interfaces;
using UniTASPlugin.Interfaces.Update;
using UniTASPlugin.MonoBehaviourController;
using UniTASPlugin.UnitySafeWrappers.Interfaces;

namespace UniTASPlugin.Trackers.SceneIndexNameTracker;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class SceneIndexNameTracker : IPluginInitialLoad, ISceneIndexName, IOnUpdate
{
    private readonly List<SceneInfo> _sceneInfos = new();

    private readonly IMonoBehaviourController _monoBehaviourController;
    private readonly ISceneWrapper _sceneWrapper;

    private int? _testingSceneIndex;
    private int _sceneCount;

    public SceneIndexNameTracker(IMonoBehaviourController monoBehaviourController, ISceneWrapper sceneWrapper)
    {
        _monoBehaviourController = monoBehaviourController;
        _sceneWrapper = sceneWrapper;
    }

    public void OnInitialLoad()
    {
        // when iterating through all scenes, we must pause execution of mono beh
        _sceneCount = _sceneWrapper.TotalSceneCount;
        _monoBehaviourController.PausedExecution = true;
        _testingSceneIndex = 0;
    }

    public void Update()
    {
        if (_testingSceneIndex == null)
        {
            return;
        }

        // load scene
        _sceneWrapper.LoadScene(_testingSceneIndex.Value);

        // get scene info
        _sceneInfos.Add(new(_testingSceneIndex.Value, _sceneWrapper.ActiveSceneName));

        if (_testingSceneIndex.Value == _sceneCount - 1)
        {
            // done
            _monoBehaviourController.PausedExecution = false;
            _testingSceneIndex = null;
        }
        else
        {
            // next scene
            _testingSceneIndex++;
        }
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