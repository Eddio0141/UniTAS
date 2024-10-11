using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using HarmonyLib;
using UniTAS.Patcher.Extensions;
using UniTAS.Patcher.Interfaces.UnitySafeWrappers;
using UniTAS.Patcher.Models.UnitySafeWrappers.Unity.Collections;

namespace UniTAS.Patcher.Implementations.UnitySafeWrappers.Unity.Collections;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class NativeArrayWrapper<T> : UnityInstanceWrap
{
    private readonly Type _allocator;
    private readonly Type _nativeArrayOptions;
    private readonly MethodBase _toArray;
    private readonly MethodBase _dispose;

    public NativeArrayWrapper(object instance) : base(instance)
    {
        var genericArg = typeof(T);
        var wrappedType = AccessTools.TypeByName("Unity.Collections.NativeArray`1")?.MakeGenericType(genericArg) ??
                          AccessTools.TypeByName("UnityEngine.Collections.NativeArray`1")?.MakeGenericType(genericArg);
        _allocator = AccessTools.TypeByName("Unity.Collections.Allocator") ??
                     AccessTools.TypeByName("UnityEngine.Collections.Allocator");
        _nativeArrayOptions = AccessTools.TypeByName("Unity.Collections.NativeArrayOptions");
        _toArray = AccessTools.Method(wrappedType, "ToArray");
        _dispose = AccessTools.Method(wrappedType, "Dispose");
        WrappedType = wrappedType;
    }

    public NativeArrayWrapper(int length, object allocator) : this(null)
    {
        if (allocator.GetType() != _allocator)
        {
            throw new ArgumentException($"Allocator must be the type: {_allocator.SaneFullName()}", nameof(allocator));
        }

        PostConstructor(length, allocator);
    }

    private void PostConstructor(int length, object allocator)
    {
        var allocatorParsed = Enum.Parse(_allocator, allocator.ToString());

        if (_nativeArrayOptions == null)
        {
            Instance = AccessTools.Constructor(WrappedType, [typeof(int), _allocator])
                .Invoke([length, allocatorParsed]);
        }
        else
        {
            var options = Enum.Parse(_nativeArrayOptions, "ClearMemory");
            Instance = AccessTools.Constructor(WrappedType, [typeof(int), _allocator, _nativeArrayOptions])
                .Invoke([length, allocatorParsed, options]);
        }
    }

    protected override Type WrappedType { get; }

    public T[] ToArray()
    {
        if (Instance == null) return null;
        return (T[])_toArray.Invoke(Instance, []);
    }

    public void Dispose()
    {
        if (Instance == null) return;
        _dispose.Invoke(Instance, []);
    }
}