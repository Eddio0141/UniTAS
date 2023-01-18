using HarmonyLib;
using UniTASPlugin.UnitySafeWrappers.Interfaces.SceneManagement;

namespace UniTASPlugin.UnitySafeWrappers.Wrappers.SceneManagement;

// ReSharper disable once UnusedType.Global
public class LoadSceneParametersWrapper : ILoadSceneParametersWrapper
{
    private readonly Traverse _instanceTraverse;
    private const string LoadSceneModeField = "loadSceneMode";
    private const string LocalPhysicsModeField = "localPhysicsMode";

    public LoadSceneParametersWrapper(object instance)
    {
        Instance = instance;
        _instanceTraverse = Traverse.Create(instance);
    }

    public object Instance { get; }

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