using System;
using StructureMap;
using UniTAS.Patcher.Implementations.UnitySafeWrappers.SceneManagement;
using UniTAS.Patcher.Implementations.UnitySafeWrappers.Unity.Collections;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Services.UnitySafeWrappers;
using UniTAS.Patcher.Services.UnitySafeWrappers.Wrappers;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.UnitySafeWrappers;

// ReSharper disable once ClassNeverInstantiated.Global
[Singleton]
[ExcludeRegisterIfTesting]
public class UnityInstanceWrapFactory(IContainer container) : IUnityInstanceWrapFactory
{
    public T Create<T>(object instance) where T : class
    {
        // because the wrap could have dependencies, we need to use the container to create it, and pass the instance to the constructor
        return container.With(instance).GetInstance<T>();
    }

    public T CreateNew<T>(params object[] args) where T : class
    {
        // TODO: this shit is stupid i hate this, what do i even do
        if (typeof(T) == typeof(LoadSceneParametersWrapper))
        {
            return new LoadSceneParametersWrapper(null) as T;
        }

        if (typeof(T) == typeof(SceneWrapper))
        {
            return new SceneWrapper(null) as T;
        }

        if (typeof(T) == typeof(RefreshRateWrap))
        {
            var newRr = new RefreshRateWrap(null);

            if (args.Length == 0)
            {
                return newRr as T;
            }

            if (args.Length == 1)
            {
                if (args[0] is double d)
                {
                    newRr.Rate = d;
                    return newRr as T;
                }

                throw new ArgumentException();
            }

            newRr.Denominator = (uint)args[1];
            newRr.Numerator = (uint)args[0];
            return newRr as T;
        }

        if (typeof(T) == typeof(IResolutionWrapper))
        {
            var newRr = new ResolutionWrapper(new Resolution());
            if (args.Length == 0)
            {
                return newRr as T;
            }

            newRr.Width = (int)args[0];
            newRr.Height = (int)args[1];
            newRr.RefreshRateWrap = (RefreshRateWrap)args[2];
            return newRr as T;
        }

        if (typeof(T) == typeof(FullScreenModeWrap))
        {
            return new FullScreenModeWrap(null) as T;
        }

        // NativeArrayWrapper<T>, i don't care whatever, fix when more generics shit
        var tGenerics = typeof(T).GetGenericArguments();
        var newNativeArray = typeof(NativeArrayWrapper<>).MakeGenericType(tGenerics);
        return Activator.CreateInstance(newNativeArray, args) as T;
    }
}