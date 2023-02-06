using UniTASPlugin.UnitySafeWrappers.Interfaces;

namespace UniTASPlugin.UnitySafeWrappers.Wrappers;

public class UnityWrapper : IUnityWrapper
{
    public IObjectWrapper Object { get; }
    public IMonoBehaviourWrapper MonoBehaviour { get; }
    public ISceneWrapper SceneWrapper { get; }

    public UnityWrapper(IObjectWrapper objectWrapper, IMonoBehaviourWrapper monoBehaviourWrapper,
        ISceneWrapper sceneWrapper)
    {
        Object = objectWrapper;
        MonoBehaviour = monoBehaviourWrapper;
        SceneWrapper = sceneWrapper;
    }
}