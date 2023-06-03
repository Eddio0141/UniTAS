using UniTAS.Patcher.Implementations.UnitySafeWrappers.Unity.Collections;

namespace UniTAS.Patcher.Services.UnitySafeWrappers.Wrappers;

public interface IAudioRendererWrapper
{
    bool Available { get; }
    int GetSampleCountForCaptureFrame { get; }
    bool Render<T>(NativeArrayWrapper<T> nativeArray);
    bool Start();
    bool Stop();
}