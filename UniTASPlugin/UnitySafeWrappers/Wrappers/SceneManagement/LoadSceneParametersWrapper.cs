using System;
using HarmonyLib;

namespace UniTASPlugin.UnitySafeWrappers.Wrappers.SceneManagement;

public class LoadSceneParametersWrapper : UnityInstanceWrap
{
    private Traverse _instanceTraverse;

    protected override Type WrappedType { get; } =
        AccessTools.TypeByName("UnityEngine.SceneManagement.LoadSceneParameters");

    private const string LoadSceneModeField = "loadSceneMode";
    private const string LocalPhysicsModeField = "localPhysicsMode";

    public override void NewInstance(params object[] args)
    {
        base.NewInstance(args);
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
            var value = _instanceTraverse.Property(LoadSceneModeField).GetValue();
            if (value == null) return null;
            return (LoadSceneMode)(int)value;
        }
        set
        {
            if (value == null) return;
            var intValue = (int)value.Value;
            _instanceTraverse.Property(LoadSceneModeField).SetValue(intValue);
        }
    }

    public LocalPhysicsMode? LocalPhysicsMode
    {
        get
        {
            var value = _instanceTraverse.Property(LocalPhysicsModeField).GetValue();
            if (value == null) return null;
            return (LocalPhysicsMode)(int)value;
        }
        set
        {
            if (value == null) return;
            var intValue = (int)value.Value;
            _instanceTraverse.Property(LocalPhysicsModeField).SetValue(intValue);
        }
    }
}