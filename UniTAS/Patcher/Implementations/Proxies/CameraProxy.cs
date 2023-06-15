using MoonSharp.Interpreter;
using StructureMap;
using UniTAS.Patcher.Interfaces.Movie;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.Proxies;

public class CameraProxy : MovieProxyType<CameraProxy, Camera>
{
    private readonly Camera _target;

    [DefaultConstructor]
    [MoonSharpHidden]
    public CameraProxy()
    {
    }

    [MoonSharpHidden]
    public CameraProxy(Camera target)
    {
        _target = target;
    }

    protected override CameraProxy CreateProxyObject(Camera target)
    {
        return new(target);
    }

    public TransformProxy Transform => new(_target.transform);
}