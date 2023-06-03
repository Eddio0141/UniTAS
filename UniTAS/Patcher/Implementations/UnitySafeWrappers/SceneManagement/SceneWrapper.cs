using System;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using UniTAS.Patcher.Interfaces.UnitySafeWrappers;

namespace UniTAS.Patcher.Implementations.UnitySafeWrappers.SceneManagement;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class SceneWrapper : UnityInstanceWrap
{
    private Traverse _instanceTraverse;

    private const string BUILD_INDEX_FIELD = "buildIndex";
    private const string NAME_FIELD = "name";

    protected override Type WrappedType { get; } = AccessTools.TypeByName("UnityEngine.SceneManagement.Scene");

    public SceneWrapper(object instance) : base(instance)
    {
        if (instance == null) return;
        _instanceTraverse = Traverse.Create(instance);
    }

    public override void NewInstance(params object[] args)
    {
        base.NewInstance(args);
        if (Instance == null) return;
        _instanceTraverse = Traverse.Create(Instance);
    }

    public int? BuildIndex => _instanceTraverse?.Property(BUILD_INDEX_FIELD).GetValue<int>();
    public string Name => _instanceTraverse?.Property(NAME_FIELD).GetValue<string>();
}