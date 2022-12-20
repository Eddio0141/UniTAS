using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Mono.Cecil;
using Mono.Cecil.Cil;
using UniTASPlugin.FixedUpdateSync;
using UniTASPlugin.GameEnvironment;
using UniTASPlugin.LegacyExceptions;
using UniTASPlugin.LegacySafeWrappers;
using UniTASPlugin.UnitySafeWrappers.Interfaces;

namespace UniTASPlugin.GameRestart;

// ReSharper disable once ClassNeverInstantiated.Global
public class GameRestart : IGameRestart
{
    private DateTime softRestartTime;

    private readonly IVirtualEnvironmentFactory _virtualEnvironmentFactory;
    private readonly ISyncFixedUpdate _syncFixedUpdate;
    private readonly IUnityWrapper _unityWrapper;

    private readonly List<KeyValuePair<Type, List<StaticFieldStorage>>> _staticFields = new();
    private readonly List<Type> _dontDestroyOnLoads = new();

    // ReSharper disable StringLiteralTypo
    private static readonly string[] gameAssemblyNames =
    {
        "Assembly-CSharp",
        "Assembly-CSharp-firstpass",
        "Assembly-UnityScript",
        "Assembly-UnityScript-firstpass"
    };

    // ReSharper restore StringLiteralTypo

    public bool PendingRestart { get; private set; }

    public GameRestart(IVirtualEnvironmentFactory virtualEnvironmentFactory, ISyncFixedUpdate syncFixedUpdate,
        IUnityWrapper unityWrapper)
    {
        _virtualEnvironmentFactory = virtualEnvironmentFactory;
        _syncFixedUpdate = syncFixedUpdate;
        _unityWrapper = unityWrapper;

        StoreStaticFields();
        StoreDontDestroyOnLoads();
    }

    private void StoreDontDestroyOnLoads()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var objectTypeName = _unityWrapper.Object.ObjectType.FullName;

        foreach (var assembly in assemblies)
        {
            if (!gameAssemblyNames.Contains(assembly.GetName().Name))
            {
                continue;
            }

            var assemblyDefinition = AssemblyDefinition.ReadAssembly(assembly.Location);
            var types = assemblyDefinition.MainModule.Types;

            // get all types that use DontDestroyOnLoad
            foreach (var type in types)
            {
                var methods = type.Methods;
                foreach (var method in methods)
                {
                    var instructions = method.Body.Instructions;
                    if (!instructions.Any(instruction =>
                            instruction.OpCode == OpCodes.Call &&
                            instruction.Operand is MethodReference { Name: "DontDestroyOnLoad" } methodReference &&
                            methodReference.DeclaringType.FullName == objectTypeName &&
                            methodReference.Parameters.Count == 1 &&
                            methodReference.Parameters[0].ParameterType.FullName ==
                            objectTypeName &&
                            methodReference.ReturnType.Name == "Void")) continue;

                    Plugin.Log.LogDebug($"Found DontDestroyOnLoad type: {type.FullName}");
                    var dontDestroyOnLoadType = assembly.GetType(type.FullName);
                    _dontDestroyOnLoads.Add(dontDestroyOnLoadType);
                    break;
                }
            }
        }
    }

    private void DestroyDontDestroyOnLoads()
    {
        foreach (var type in _dontDestroyOnLoads)
        {
            var dontDestroyOnLoadObjects = _unityWrapper.Object.FindObjectsOfType(type);
            foreach (var dontDestroyOnLoadObject in dontDestroyOnLoadObjects)
            {
                _unityWrapper.Object.Destroy(dontDestroyOnLoadObject);
            }
        }
    }

    // TODO remove this when we have a better way to handle static fields
    private void StoreStaticFields()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var assembly in assemblies)
        {
            if (!gameAssemblyNames.Contains(assembly.GetName().Name))
            {
                continue;
            }

            var types = assembly.GetTypes();
            foreach (var type in types)
            {
                var fields = type.GetFields(AccessTools.all);
                if (fields.Length == 0)
                {
                    continue;
                }

                var staticFields = new List<StaticFieldStorage>();
                foreach (var field in fields)
                {
                    // ignore if not static or const
                    if (!field.IsStatic || field.IsLiteral)
                    {
                        continue;
                    }

                    var value = field.GetValue(null);
                    // TODO remove hardcoded dependency
                    Plugin.Log.LogDebug(
                        $"Cloning and storing static field {type.FullName}.{field.Name} with value " + (value == null
                            ? "null"
                            : value.ToString()));
                    object valueClone = null;
                    try
                    {
                        valueClone = Helper.MakeDeepCopy(value, field.FieldType);
                    }
                    catch (DeepCopyMaxRecursion)
                    {
                        // ignore it
                    }

                    var staticFieldStorage = new StaticFieldStorage(field, valueClone);
                    staticFields.Add(staticFieldStorage);
                }

                _staticFields.Add(new(type, staticFields));
            }
        }
    }

    private void SetStaticFields()
    {
        foreach (var typeAndStaticFields in _staticFields)
        {
            var type = typeAndStaticFields.Key;
            var staticFields = typeAndStaticFields.Value;
            foreach (var staticField in staticFields)
            {
                // TODO remove hardcoded dependency
                Plugin.Log.LogDebug(
                    $"Setting static field {type.FullName}.{staticField.Field.Name} to value {staticField.Value}");
                // TODO remove hardcoded dependency
                var fieldClone = Helper.MakeDeepCopy(staticField.Value, staticField.Field.FieldType);
                staticField.Field.SetValue(null, fieldClone);
            }
        }
    }

    /// <summary>
    /// Soft restart the game. This will not reload the game, but tries to reset the game state.
    /// </summary>
    /// <param name="time">Time to start the game at</param>
    public void SoftRestart(DateTime time)
    {
        PendingRestart = true;
        softRestartTime = time;
        DestroyDontDestroyOnLoads();
        StopScriptExecution();
        SetStaticFields();
        _syncFixedUpdate.OnSync(SoftRestartOperation, 1);
        Plugin.Log.LogInfo("Soft restarting, pending FixedUpdate call");
    }

    private void SoftRestartOperation()
    {
        Plugin.Log.LogInfo("Soft restarting");

        Plugin.Log.LogDebug("finished setting fields, loading scene");
        var env = _virtualEnvironmentFactory.GetVirtualEnv();
        env.GameTime.StartupTime = softRestartTime;
        SceneHelper.LoadScene(0);

        Plugin.Log.LogDebug("random setting state");

        RandomWrap.InitState((int)env.Seed);

        Plugin.Log.LogInfo("Finish soft restarting");
        Plugin.Log.LogInfo($"System time: {DateTime.Now}");

        PendingRestart = false;
    }

    private void StopScriptExecution()
    {
        var allMonoBehaviours =
            _unityWrapper.Object.FindObjectsOfType(_unityWrapper.MonoBehaviour.MonoBehaviourType);

        // TODO remove hardcoded dependencies
        var ignoreTypes = new[]
        {
            "UniTASPlugin.Plugin",
            "UnityEngine.EventSystems.EventSystem"
        };

        foreach (var monoBehaviour in allMonoBehaviours)
        {
            if (ignoreTypes.Contains(monoBehaviour.GetType().FullName))
            {
                continue;
            }

            _unityWrapper.MonoBehaviour.StopAllCoroutines(monoBehaviour);
            _unityWrapper.Object.Destroy(monoBehaviour);
        }
    }

    private class StaticFieldStorage
    {
        public FieldInfo Field { get; }
        public object Value { get; }

        public StaticFieldStorage(FieldInfo field, object value)
        {
            Field = field;
            Value = value;
        }
    }
}