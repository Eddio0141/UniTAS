using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Emit;
using HarmonyLib;
using MonoMod.Utils;
using UniTAS.Patcher.Extensions;
using UniTAS.Patcher.Interfaces.UnitySafeWrappers;

namespace UniTAS.Patcher.Implementations.UnitySafeWrappers.SceneManagement;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class SceneWrapper(object instance) : UnityInstanceWrap(instance)
{
    protected override Type WrappedType { get; } = AccessTools.TypeByName("UnityEngine.SceneManagement.Scene");

    public bool IsValid => IsValidMethod(Instance);
    public string Name => NameGetter(Instance);
    public string Path => PathGetter(Instance);
    public bool IsLoaded => IsLoadedGetter(Instance);
    public int BuildIndex => BuildIndexGetter(Instance);
    // public bool IsDirty => IsDirtyGetter(Instance);
    // public int RootCount => RootCountGetter(Instance);

    public int Handle
    {
        get => GetHandleField(Instance);
        set
        {
            var i = Instance;
            SetHandleField(ref i, value);
            Instance = i;
        }
    }

    // TODO: when does this exist from?
    public bool IsSubScene => IsSubSceneGetter(Instance);

    private static readonly Type SceneType = AccessTools.TypeByName("UnityEngine.SceneManagement.Scene");
    private static readonly Func<object, string> NameGetter;
    private static readonly Func<object, string> PathGetter;
    private static readonly Func<object, int> BuildIndexGetter;
    private static readonly Func<object, bool> IsValidMethod;
    private static readonly Func<object, bool> IsLoadedGetter;
    // private static readonly Func<object, bool> IsDirtyGetter;
    private static readonly Func<object, bool> IsSubSceneGetter;
    // private static readonly Func<object, int> RootCountGetter;
    private static readonly Func<object, int> GetHandleField;
    private static readonly SetHandleFieldDelegate SetHandleField;

    private delegate void SetHandleFieldDelegate(ref object instance, int handle);

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
        // var isDirty = AccessTools.PropertyGetter(SceneType, "isDirty");
        // IsDirtyGetter = isDirty.MethodDelegate<Func<object, bool>>();
        // var rootCount = AccessTools.PropertyGetter(SceneType, "rootCount");
        // RootCountGetter = rootCount.MethodDelegate<Func<object, int>>();
        var isSubScene = AccessTools.PropertyGetter(SceneType, "isSubScene");
        // TODO: does it exist
        IsSubSceneGetter = isSubScene?.MethodDelegate<Func<object, bool>>();

        var handleField = AccessTools.Field(SceneType, "m_Handle");
        var dmd = new DynamicMethodDefinition("m_Handle_get", typeof(int), [typeof(object)]);
        var il = dmd.GetILGenerator();
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Unbox_Any, SceneType);
        il.Emit(OpCodes.Ldfld, handleField);
        il.Emit(OpCodes.Ret);
        GetHandleField = dmd.Generate().CreateDelegate<Func<object, int>>();

        dmd = new DynamicMethodDefinition("m_Handle_set", null, [typeof(object).MakeByRefType(), typeof(int)]);
        il = dmd.GetILGenerator();
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldind_Ref);
        il.Emit(OpCodes.Unbox, SceneType);
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Stfld, handleField);
        il.Emit(OpCodes.Ret);
        SetHandleField = dmd.Generate().CreateDelegate<SetHandleFieldDelegate>();
    }
}