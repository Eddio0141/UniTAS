using UniTASPlugin.UnitySafeWrappers.Interfaces;

namespace UniTASPlugin.UnitySafeWrappers.Wrappers;

// ReSharper disable once ClassNeverInstantiated.Global
public class UnityWrapper : IUnityWrapper
{
    public ISceneWrapper Scene { get; }
    public IRandomWrapper Random { get; }
    public ITimeWrapper Time { get; }

    public UnityWrapper(ISceneWrapper sceneWrapper, IRandomWrapper random, ITimeWrapper time)
    {
        Scene = sceneWrapper;
        Random = random;
        Time = time;
    }
}