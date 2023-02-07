namespace UniTASPlugin.UnitySafeWrappers.Interfaces;

public interface IUnityWrapper
{
    IObjectWrapper Object { get; }
    IMonoBehaviourWrapper MonoBehaviour { get; }
    ISceneWrapper SceneWrapper { get; }
    IRandomWrapper Random { get; }
}