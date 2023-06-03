using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using HarmonyLib;
using UniTAS.Patcher.Implementations.UnitySafeWrappers.Unity.Collections;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Services.UnitySafeWrappers.Wrappers;

namespace UniTAS.Patcher.Implementations.UnitySafeWrappers.UnityEngine;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[Singleton]
public class AudioRendererWrapper : IAudioRendererWrapper
{
    public bool Available { get; }

    private readonly Func<int> _getSampleCountForCaptureFrame;
    private readonly MethodBase _render;
    private readonly Func<bool> _start;
    private readonly Func<bool> _stop;

    private readonly object[] _renderArgCache = new object[1];

    public AudioRendererWrapper()
    {
        var audioRendererType = AccessTools.TypeByName("UnityEngine.AudioRenderer");
        if (audioRendererType == null) return;

        var getSampleCountForCaptureFrame = AccessTools.Method(audioRendererType, "GetSampleCountForCaptureFrame");
        if (getSampleCountForCaptureFrame == null) return;

        _render = AccessTools.Method(audioRendererType, "Render");
        if (_render == null) return;

        var start = AccessTools.Method(audioRendererType, "Start");
        if (start == null) return;

        var stop = AccessTools.Method(audioRendererType, "Stop");
        if (stop == null) return;

        Available = true;

        _getSampleCountForCaptureFrame = AccessTools.MethodDelegate<Func<int>>(getSampleCountForCaptureFrame);
        _start = AccessTools.MethodDelegate<Func<bool>>(start);
        _stop = AccessTools.MethodDelegate<Func<bool>>(stop);
    }

    public int GetSampleCountForCaptureFrame => _getSampleCountForCaptureFrame();

    public bool Render<T>(NativeArrayWrapper<T> nativeArray)
    {
        _renderArgCache[0] = nativeArray.Instance;
        return (bool)_render.Invoke(null, _renderArgCache);
    }

    public bool Start()
    {
        return _start();
    }

    public bool Stop()
    {
        return _stop();
    }
}