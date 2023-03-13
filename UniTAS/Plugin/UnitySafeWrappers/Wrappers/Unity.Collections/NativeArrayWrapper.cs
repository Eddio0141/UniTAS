using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using HarmonyLib;

namespace UniTAS.Plugin.UnitySafeWrappers.Wrappers.Unity.Collections;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class NativeArrayWrapper<T> : UnityInstanceWrap
{
    private readonly Type _allocator;
    private readonly MethodBase _toArray;

    public NativeArrayWrapper(object instance) : base(instance)
    {
        var genericArg = typeof(T);
        var wrappedType = AccessTools.TypeByName("Unity.Collections.NativeArray`1").MakeGenericType(genericArg);
        _allocator = AccessTools.TypeByName("Unity.Collections.Allocator");
        _toArray = AccessTools.Method(wrappedType, "ToArray");
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
        return (T[])_toArray.Invoke(Instance, new object[0]);
    }
}