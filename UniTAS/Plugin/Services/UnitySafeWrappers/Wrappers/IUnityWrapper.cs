namespace UniTAS.Plugin.Services.UnitySafeWrappers.Wrappers;

public interface IUnityWrapper
{
    ISceneWrapper Scene { get; }
    IRandomWrapper Random { get; }
    ITimeWrapper Time { get; }
}