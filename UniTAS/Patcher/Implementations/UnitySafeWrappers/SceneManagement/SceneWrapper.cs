using System;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using UniTAS.Patcher.Interfaces.UnitySafeWrappers;

namespace UniTAS.Patcher.Implementations.UnitySafeWrappers.SceneManagement;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class SceneWrapper : UnityInstanceWrap
{
    private readonly Traverse _instanceTraverse;

    private const string BuildIndexField = "buildIndex";
    private const string NameField = "name";

    protected override Type WrappedType { get; } = AccessTools.TypeByName("UnityEngine.SceneManagement.Scene");

    public SceneWrapper(object instance) : base(instance)
    {
        if (instance == null) return;
        _instanceTraverse = Traverse.Create(instance);
    }

    public int? BuildIndex => _instanceTraverse?.Property(BuildIndexField).GetValue<int>();
    public string Name => _instanceTraverse?.Property(NameField).GetValue<string>();
}