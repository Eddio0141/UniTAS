using System;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;

namespace UniTAS.Plugin.UnitySafeWrappers.Wrappers.Unity.Collections;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class NativeArrayWrapper<T> : UnityInstanceWrap
{
    private readonly Type _allocator;
    private readonly Func<T[]> _toArray;

    public NativeArrayWrapper(object instance) : base(instance)
    {
        var genericArg = typeof(T);
        var wrappedType = AccessTools.TypeByName("UnityEngine.NativeArray`1").MakeGenericType(genericArg);
        _allocator = AccessTools.TypeByName("Unity.Collections.Allocator");
        _toArray = AccessTools.MethodDelegate<Func<T[]>>(AccessTools.Method(wrappedType, "ToArray"), Instance);
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

        base.NewInstance(args);
    }

    protected override Type WrappedType { get; }

    public T[] ToArray()
    {
        return _toArray();
    }
}