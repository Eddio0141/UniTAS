using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using HarmonyLib;
using UniTAS.Patcher.Extensions;
using UniTAS.Patcher.Interfaces.UnitySafeWrappers;
using UniTAS.Patcher.Services;

namespace UniTAS.Patcher.Implementations.UnitySafeWrappers.SceneManagement;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class SceneWrapper(object instance, IPatchReverseInvoker patchReverseInvoker) : UnityInstanceWrap(instance)
{
    protected override Type WrappedType { get; } = AccessTools.TypeByName("UnityEngine.SceneManagement.Scene");

    public bool IsValid => patchReverseInvoker.Invoke((m, i) => m(i), IsValidMethod, Instance);
    public string Name => patchReverseInvoker.Invoke((m, i) => m(i), NameGetter, Instance);
    public string Path => patchReverseInvoker.Invoke((m, i) => m(i), PathGetter, Instance);
    public bool IsLoaded => patchReverseInvoker.Invoke((m, i) => m(i), IsLoadedGetter, Instance);
    public int BuildIndex => patchReverseInvoker.Invoke((m, i) => m(i), BuildIndexGetter, Instance);
    public bool IsDirty => patchReverseInvoker.Invoke((m, i) => m(i), IsDirtyGetter, Instance);
    public int RootCount => patchReverseInvoker.Invoke((m, i) => m(i), RootCountGetter, Instance);

    public int Handle
    {
        get => (int)HandleField.GetValue(Instance);
        set
        {
            var i = Instance;
            HandleField.SetValue(i, value);
            Instance = i;
        }
    }

    // TODO: when does this exist from?
    public bool IsSubScene => patchReverseInvoker.Invoke((m, i) => m?.Invoke(i) ?? false, IsSubSceneGetter, Instance);

    private static readonly Type SceneType = AccessTools.TypeByName("UnityEngine.SceneManagement.Scene");
    private static readonly Func<object, string> NameGetter;
    private static readonly Func<object, string> PathGetter;
    private static readonly Func<object, int> BuildIndexGetter;
    private static readonly Func<object, bool> IsValidMethod;
    private static readonly Func<object, bool> IsLoadedGetter;
    private static readonly Func<object, bool> IsDirtyGetter;
    private static readonly Func<object, bool> IsSubSceneGetter;
    private static readonly Func<object, int> RootCountGetter;
    private static readonly FieldInfo HandleField;

    static SceneWrapper()
    {
        if (SceneType == null) return;
        var name = AccessTools.PropertyGetter(SceneType, "name");
        NameGetter = name.MethodDelegate<Func<object, string>>();
        var path = AccessTools.PropertyGetter(SceneType, "path");
        PathGetter = path.MethodDelegate<Func<object, string>>();
        var buildIndex = AccessTools.PropertyGetter(SceneType, "buildIndex");
        BuildIndexGetter = buildIndex.MethodDelegate<Func<object, int>>();
        var isValid = AccessTools.Method(SceneType, "IsValid");
        IsValidMethod = isValid.MethodDelegate<Func<object, bool>>();
        var isLoaded = AccessTools.PropertyGetter(SceneType, "isLoaded");
        IsLoadedGetter = isLoaded.MethodDelegate<Func<object, bool>>();
        var isDirty = AccessTools.PropertyGetter(SceneType, "isDirty");
        IsDirtyGetter = isDirty.MethodDelegate<Func<object, bool>>();
        var rootCount = AccessTools.PropertyGetter(SceneType, "rootCount");
        RootCountGetter = rootCount.MethodDelegate<Func<object, int>>();
        HandleField = AccessTools.Field(SceneType, "m_Handle");
        var isSubScene = AccessTools.PropertyGetter(SceneType, "isSubScene");
        // TODO: does it exist
        IsSubSceneGetter = isSubScene?.MethodDelegate<Func<object, bool>>();
    }
}