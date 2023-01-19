using System;
using HarmonyLib;
using UniTASPlugin.UnitySafeWrappers.Interfaces.SceneManagement;

namespace UniTASPlugin.UnitySafeWrappers.Wrappers.SceneManagement;

// ReSharper disable once UnusedType.Global
public class LoadSceneParametersWrapper : ILoadSceneParametersWrapper
{
    private Traverse _instanceTraverse;
    private object _instance;
    private const string LoadSceneModeField = "loadSceneMode";
    private const string LocalPhysicsModeField = "localPhysicsMode";

    private readonly Type _loadSceneParametersType =
        AccessTools.TypeByName("UnityEngine.SceneManagement.LoadSceneParameters");

    public void CreateInstance()
    {
        Instance = Activator.CreateInstance(_loadSceneParametersType);
    }

    public object Instance
    {
        get => _instance;
        set
        {
            _instanceTraverse = Traverse.Create(value);
            _instance = value;
        }
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