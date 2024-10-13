using System;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using UniTAS.Patcher.Interfaces.UnitySafeWrappers;
using UniTAS.Patcher.Models.UnitySafeWrappers.SceneManagement;

namespace UniTAS.Patcher.Implementations.UnitySafeWrappers.SceneManagement;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class LoadSceneParametersWrapper(object instance) : UnityInstanceWrap(instance)
{
    private readonly Traverse _instanceTraverse;

    public LoadSceneParametersWrapper(object instance) : base(instance)
    {
        _instanceTraverse = Traverse.Create(Instance);
    }

    protected override Type WrappedType { get; } =
        AccessTools.TypeByName("UnityEngine.SceneManagement.LoadSceneParameters");

    private const string LoadSceneModeField = "loadSceneMode";
    private const string LocalPhysicsModeField = "localPhysicsMode";

    public LoadSceneMode? LoadSceneMode
    {
        get
        {
            var value = _instanceTraverse?.Property(LoadSceneModeField).GetValue();
            if (value == null) return null;
            return (LoadSceneMode)(int)value;
        }
        set
        {
            if (value == null || _instanceTraverse == null) return;
            var intValue = (int)value.Value;
            _instanceTraverse.Property(LoadSceneModeField).SetValue(intValue);
        }
    }

    public LocalPhysicsMode? LocalPhysicsMode
    {
        get
        {
            var value = _instanceTraverse?.Property(LocalPhysicsModeField).GetValue();
            if (value == null) return null;
            return (LocalPhysicsMode)(int)value;
        }
        set
        {
            if (value == null || _instanceTraverse == null) return;
            var intValue = (int)value.Value;
            _instanceTraverse.Property(LocalPhysicsModeField).SetValue(intValue);
        }
    }
