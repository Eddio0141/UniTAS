using UniTAS.Patcher.Services.RuntimeTest;
using UniTAS.Patcher.Services.VirtualEnvironment;
using UnityEngine;

namespace UniTAS.Patcher.Models.RuntimeTest;

public class UnityEnvTestingSave : IUnityEnvTestingSave
{
    private double _originalFt;
    private float _originalFixedDt;
    private float _originalMaxDt;
    private float _originalTimeScale;

    private readonly ITimeEnv _timeEnv;

    public UnityEnvTestingSave(ITimeEnv timeEnv)
    {
        _timeEnv = timeEnv;
    }

    public void Save()
    {
        _originalFt = _timeEnv.FrameTime;
        _originalFixedDt = Time.fixedDeltaTime;
        _originalMaxDt = Time.maximumDeltaTime;
        _originalTimeScale = Time.timeScale;
    }

    public void Restore()
    {
        _timeEnv.FrameTime = _originalFt;
        Time.fixedDeltaTime = _originalFixedDt;
        Time.maximumDeltaTime = _originalMaxDt;
        Time.timeScale = _originalTimeScale;
    }
}