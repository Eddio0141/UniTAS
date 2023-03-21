using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using HarmonyLib;
using UniTAS.Plugin.Interfaces.UnitySafeWrappers;
using UniTAS.Plugin.Models.UnitySafeWrappers.Unity.Collections;

namespace UniTAS.Plugin.Implementations.UnitySafeWrappers.Unity.Collections;

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
        var wrappedType = AccessTools.TypeByName("Unity.Collections.NativeArray`1").MakeGenericType(genericArg);
        _allocator = AccessTools.TypeByName("Unity.Collections.Allocator");
        _nativeArrayOptions = AccessTools.TypeByName("Unity.Collections.NativeArrayOptions");
        _toArray = AccessTools.Method(wrappedType, "ToArray");
        _dispose = AccessTools.Method(wrappedType, "Dispose");
        WrappedType = wrappedType;
    }

    public override void NewInstance(params object[] args)
    {
        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            if (arg is Allocator allocator)
            {
                args[i] = Enum.Parse(_allocator, allocator.ToString());
            }
        }

        if (args.Length == 2 && args[0] is int && args[1].GetType() == _allocator)
        {
            args = new[] { args[0], args[1], Enum.Parse(_nativeArrayOptions, "ClearMemory") };
            Instance = AccessTools.Constructor(WrappedType, new[] { typeof(int), _allocator, _nativeArrayOptions })
                .Invoke(args);
        }
        else
        {
            base.NewInstance(args);
        }
    }

    protected override Type WrappedType { get; }

    public T[] ToArray()
    {
        return (T[])_toArray.Invoke(Instance, new object[0]);
    }

    public void Dispose()
    {
        _dispose.Invoke(Instance, new object[0]);
    }
}