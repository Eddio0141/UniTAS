using UniTAS.Plugin.UnitySafeWrappers.Wrappers.Unity.Collections;

namespace UniTAS.Plugin.Services.UnitySafeWrappers.Wrappers;

public interface IAudioRendererWrapper
{
    bool Available { get; }
    int GetSampleCountForCaptureFrame { get; }
    bool Render<T>(NativeArrayWrapper<T> nativeArray);
    bool Start();
    bool Stop();
}