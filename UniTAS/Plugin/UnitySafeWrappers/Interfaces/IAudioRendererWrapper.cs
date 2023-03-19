using UniTAS.Plugin.UnitySafeWrappers.Wrappers.Unity.Collections;

namespace UniTAS.Plugin.UnitySafeWrappers.Interfaces;

public interface IAudioRendererWrapper
{
    bool Available { get; }
    int GetSampleCountForCaptureFrame { get; }
    bool Render<T>(NativeArrayWrapper<T> nativeArray);
    bool Start();
    bool Stop();
}