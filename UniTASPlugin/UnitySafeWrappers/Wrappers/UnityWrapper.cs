using UniTASPlugin.UnitySafeWrappers.Interfaces;

namespace UniTASPlugin.UnitySafeWrappers.Wrappers;

// ReSharper disable once ClassNeverInstantiated.Global
public class UnityWrapper : IUnityWrapper
{
    public IObjectWrapper Object { get; }
    public IMonoBehaviourWrapper MonoBehaviour { get; }
    public ISceneWrapper Scene { get; }
    public IRandomWrapper Random { get; }
    public ITimeWrapper Time { get; }

    public UnityWrapper(IObjectWrapper objectWrapper, IMonoBehaviourWrapper monoBehaviourWrapper,
        ISceneWrapper sceneWrapper, IRandomWrapper random, ITimeWrapper time)
    {
        Object = objectWrapper;
        MonoBehaviour = monoBehaviourWrapper;
        Scene = sceneWrapper;
        Random = random;
        Time = time;
    }
}