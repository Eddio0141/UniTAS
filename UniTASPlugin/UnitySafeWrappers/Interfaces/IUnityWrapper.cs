namespace UniTASPlugin.UnitySafeWrappers.Interfaces;

public interface IUnityWrapper
{
    IObjectWrapper Object { get; }
    IMonoBehaviourWrapper MonoBehaviour { get; }
    ISceneWrapper Scene { get; }
    IRandomWrapper Random { get; }
    ITimeWrapper Time { get; }
}