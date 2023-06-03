using System;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using UniTAS.Plugin.Interfaces.UnitySafeWrappers;
using UniTAS.Plugin.Models.UnitySafeWrappers.SceneManagement;

namespace UniTAS.Plugin.Implementations.UnitySafeWrappers.SceneManagement;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class LoadSceneParametersWrapper : UnityInstanceWrap
{
    private Traverse _instanceTraverse;

    protected override Type WrappedType { get; } =
        AccessTools.TypeByName("UnityEngine.SceneManagement.LoadSceneParameters");

    private const string LOAD_SCENE_MODE_FIELD = "loadSceneMode";
    private const string LOCAL_PHYSICS_MODE_FIELD = "localPhysicsMode";

    public override void NewInstance(params object[] args)
    {
        base.NewInstance(args);
        if (Instance == null) return;
        _instanceTraverse = Traverse.Create(Instance);
    }

    public LoadSceneParametersWrapper(object instance) : base(instance)
    {
        _instanceTraverse = Traverse.Create(instance);
    }

    public LoadSceneMode? LoadSceneMode
    {
        get
        {
            var value = _instanceTraverse?.Property(LOAD_SCENE_MODE_FIELD).GetValue();
            if (value == null) return null;
            return (LoadSceneMode)(int)value;
        }
        set
        {
            if (value == null || _instanceTraverse == null) return;
            var intValue = (int)value.Value;
            _instanceTraverse.Property(LOAD_SCENE_MODE_FIELD).SetValue(intValue);
        }
    }

    public LocalPhysicsMode? LocalPhysicsMode
    {
        get
        {
            var value = _instanceTraverse?.Property(LOCAL_PHYSICS_MODE_FIELD).GetValue();
            if (value == null) return null;
            return (LocalPhysicsMode)(int)value;
        }
        set
        {
            if (value == null || _instanceTraverse == null) return;
            var intValue = (int)value.Value;
            _instanceTraverse.Property(LOCAL_PHYSICS_MODE_FIELD).SetValue(intValue);
        }
    }
}