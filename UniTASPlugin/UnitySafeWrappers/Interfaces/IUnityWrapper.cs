namespace UniTASPlugin.UnitySafeWrappers.Interfaces;

public interface IUnityWrapper
{
    ISceneWrapper Scene { get; }
    IRandomWrapper Random { get; }
    ITimeWrapper Time { get; }
}