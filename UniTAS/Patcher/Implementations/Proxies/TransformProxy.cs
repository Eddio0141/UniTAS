using System.Diagnostics.CodeAnalysis;
using MoonSharp.Interpreter;
using UniTAS.Patcher.Interfaces.Movie;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.Proxies;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class TransformProxy : MovieProxyType<TransformProxy, Transform>
{
    private readonly Transform _transform;

    [MoonSharpHidden]
    public TransformProxy(Transform transform)
    {
        _transform = transform;
    }

    public Vector3 position => _transform.position;
    public Vector3 localPosition => _transform.localPosition;
    public Vector3 eulerAngles => _transform.eulerAngles;
    public Vector3 localEulerAngles => _transform.localEulerAngles;
    public Vector3 right => _transform.right;
    public Vector3 up => _transform.up;
    public Vector3 forward => _transform.forward;
    public Vector3 localScale => _transform.localScale;
    public TransformProxy parent => new(_transform.parent);
    public Matrix4x4 worldToLocalMatrix => _transform.worldToLocalMatrix;
    public Matrix4x4 localToWorldMatrix => _transform.localToWorldMatrix;
    public TransformProxy root => new(_transform.root);
    public int childCount => _transform.childCount;
    public Vector3 lossyScale => _transform.lossyScale;

    public TransformProxy Find(string name)
    {
        return new(_transform.Find(name));
    }

    public bool IsChildOf(TransformProxy parentProxy)
    {
        return _transform.IsChildOf(parentProxy._transform);
    }

    public TransformProxy FindChild(string name)
    {
        return new(_transform.FindChild(name));
    }

    public TransformProxy GetChild(int index)
    {
        return new(_transform.GetChild(index));
    }

    public int GetChildCount()
    {
        return _transform.GetChildCount();
    }

    protected override TransformProxy CreateProxyObject(Transform target)
    {
        return new(target);
    }
}