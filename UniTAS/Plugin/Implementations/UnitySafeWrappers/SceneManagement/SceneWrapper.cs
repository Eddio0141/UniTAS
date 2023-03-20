using System;
using HarmonyLib;
using UniTAS.Plugin.Interfaces.UnitySafeWrappers;

namespace UniTAS.Plugin.Implementations.UnitySafeWrappers.SceneManagement;

public class SceneWrapper : UnityInstanceWrap
{
    private Traverse _instanceTraverse;

    private const string BuildIndexField = "buildIndex";
    private const string NameField = "name";

    protected override Type WrappedType { get; } = AccessTools.TypeByName("UnityEngine.SceneManagement.Scene");

    public SceneWrapper(object instance) : base(instance)
    {
        _instanceTraverse = Traverse.Create(instance);
    }

    public override void NewInstance(params object[] args)
    {
        base.NewInstance(args);
        _instanceTraverse = Traverse.Create(Instance);
    }

    public int BuildIndex => _instanceTraverse.Property(BuildIndexField).GetValue<int>();
    public string Name => _instanceTraverse.Property(NameField).GetValue<string>();
}