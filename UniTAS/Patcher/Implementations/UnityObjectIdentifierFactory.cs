using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Models;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.UnitySafeWrappers.Wrappers;
using UnityEngine;

namespace UniTAS.Patcher.Implementations;

[Singleton]
[ExcludeRegisterIfTesting]
public class UnityObjectIdentifierFactory(ISceneManagerWrapper iSceneManagerWrapper) : IUnityObjectIdentifierFactory
{
    public UnityObjectIdentifier NewUnityObjectIdentifier(Object o)
    {
        return new UnityObjectIdentifier(o, this, iSceneManagerWrapper);
    }
}