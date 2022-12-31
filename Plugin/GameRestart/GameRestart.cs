using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Utils;
using UniTASPlugin.FixedUpdateSync;
using UniTASPlugin.GameEnvironment;
using UniTASPlugin.Interfaces.StartEvent;
using UniTASPlugin.Interfaces.Update;
using UniTASPlugin.LegacyExceptions;
using UniTASPlugin.LegacySafeWrappers;
using UniTASPlugin.Logger;
using UniTASPlugin.MonoBehaviourController;
using UniTASPlugin.UnitySafeWrappers.Interfaces;

namespace UniTASPlugin.GameRestart;

// ReSharper disable once ClassNeverInstantiated.Global
public class GameRestart : IGameRestart, IOnAwake, IOnEnable, IOnStart, IOnFixedUpdate
{
    private DateTime _softRestartTime;

    private readonly IVirtualEnvironmentFactory _virtualEnvironmentFactory;
    private readonly ISyncFixedUpdate _syncFixedUpdate;
    private readonly IUnityWrapper _unityWrapper;
    private readonly IMonoBehaviourController _monoBehaviourController;
    private readonly ILogger _logger;

    private readonly IOnGameRestart[] _onGameRestart;

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
    private bool _pendingResumePausedExecution;

    public GameRestart(IVirtualEnvironmentFactory virtualEnvironmentFactory, ISyncFixedUpdate syncFixedUpdate,
        IUnityWrapper unityWrapper, IMonoBehaviourController monoBehaviourController, ILogger logger,
        IOnGameRestart[] onGameRestart)
    {
        _virtualEnvironmentFactory = virtualEnvironmentFactory;
        _syncFixedUpdate = syncFixedUpdate;
        _unityWrapper = unityWrapper;
        _monoBehaviourController = monoBehaviourController;
        _logger = logger;
        _onGameRestart = onGameRestart;

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
                    if (!method.HasBody) continue;

                    var instructions = method.Body.Instructions;
                    if (!instructions.Any(instruction => instruction.OpCode == OpCodes.Call &&
                                                         instruction.Operand is MethodReference
                                                         {
                                                             Name: "DontDestroyOnLoad",
                                                             Parameters.Count : 1,
                                                             ReturnType.Name: "Void",
                                                         } methodReference &&
                                                         methodReference.DeclaringType.FullName == objectTypeName &&
                                                         methodReference.Parameters[0].ParameterType.FullName ==
                                                         objectTypeName))
                    {
                        continue;
                    }

                    _logger.LogDebug($"Found DontDestroyOnLoad type: {type.FullName}");
                    _dontDestroyOnLoads.Add(type.ResolveReflection());
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
                _unityWrapper.MonoBehaviour.StopAllCoroutines(dontDestroyOnLoadObject);
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

            {
                var assemblyDefinition = AssemblyDefinition.ReadAssembly(assembly.Location);
                var types = assemblyDefinition.MainModule.Types;

                foreach (var type in types)
                {
                    var staticFields = new List<StaticFieldStorage>();

                    var fields = type.Fields.Where(x => x.IsStatic && !x.IsLiteral).ToArray();
                    if (fields.Length == 0) continue;

                    var processedFields = new bool[fields.Length];

                    var staticConstructor = type.Methods.FirstOrDefault(x => x.IsStatic && x.IsConstructor);

                    if (staticConstructor != null)
                    {
                        if (!staticConstructor.HasBody) continue;
                        var instructions = staticConstructor.Body.Instructions;
                        foreach (var instruction in instructions)
                        {
                            // we label processedFields as true when we find a field that is set in the static constructor
                            if (instruction.OpCode == OpCodes.Stsfld &&
                                instruction.Operand is FieldReference fieldReference)
                            {
                                for (var i = 0; i < fields.Length; i++)
                                {
                                    if (fields[i].Name == fieldReference.Name)
                                    {
                                        processedFields[i] = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    for (var i = 0; i < processedFields.Length; i++)
                    {
                        var processedField = processedFields[i];
                        if (processedField) continue;
                        // lazy solution right now so we store null for fields that are not set in the static constructor
                        var field = fields[i];
                        _logger.LogDebug(
                            $"Found static field: {type.FullName}.{field.Name} with value of null");
                        staticFields.Add(new(field.ResolveReflection(), null));
                    }

                    if (staticFields.Count > 0)
                    {
                        var fieldType = type.ResolveReflection();
                        _staticFields.Add(new(fieldType, staticFields));
                    }
                }
            }

            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    var fields = type.GetFields(AccessTools.all);
                    if (fields.Length == 0) continue;

                    var typeAlreadyDefinedIndex = _staticFields.FindIndex(x => x.Key == type);

                    var staticFields = new List<StaticFieldStorage>();
                    foreach (var field in fields)
                    {
                        // ignore if not static or const
                        if (!field.IsStatic || field.IsLiteral) continue;

                        // ignore if already defined in the assembly definition
                        if (typeAlreadyDefinedIndex > -1 &&
                            _staticFields[typeAlreadyDefinedIndex].Value.Any(x => x.Field == field))
                            continue;

                        var value = field.GetValue(null);
                        _logger.LogDebug(
                            $"Cloning and storing static field {type.FullName}.{field.Name} with value " +
                            (value == null
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

                    if (staticFields.Count > 0)
                    {
                        _staticFields.Add(new(type, staticFields));
                    }
                }
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
                var valueString = staticField.Value == null
                    ? "null"
                    : staticField.Value.ToString();
                _logger.LogDebug(
                    $"Setting static field {type.FullName}.{staticField.Field.Name} to value {valueString}");
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
        _softRestartTime = time;
        DestroyDontDestroyOnLoads();
        StopScriptExecution();
        SetStaticFields();
        OnGameRestart();
        _syncFixedUpdate.OnSync(SoftRestartOperation, 1);
        _logger.LogDebug("Soft restarting, pending FixedUpdate call");
    }

    private void OnGameRestart()
    {
        foreach (var gameRestart in _onGameRestart)
        {
            gameRestart.OnGameRestart(_softRestartTime);
        }
    }

    private void SoftRestartOperation()
    {
        _logger.LogInfo("Soft restarting");

        SceneHelper.LoadScene(0);

        _logger.LogDebug("random setting state");

        var env = _virtualEnvironmentFactory.GetVirtualEnv();
        RandomWrap.InitState((int)env.Seed);

        _logger.LogInfo("Finish soft restarting");
        _logger.LogInfo($"System time: {DateTime.Now}");

        PendingRestart = false;
        _pendingResumePausedExecution = true;
    }

    private void StopScriptExecution()
    {
        _logger.LogDebug("Stopping MonoBehaviour execution");
        _monoBehaviourController.PausedExecution = true;
    }

    public void Awake()
    {
        PendingResumePausedExecution();
    }

    public void OnEnable()
    {
        PendingResumePausedExecution();
    }

    public void Start()
    {
        PendingResumePausedExecution();
    }

    // plugin will call this as a backup
    public void FixedUpdate()
    {
        PendingResumePausedExecution();
    }

    private void PendingResumePausedExecution()
    {
        if (!_pendingResumePausedExecution) return;
        _pendingResumePausedExecution = false;
        _monoBehaviourController.PausedExecution = false;
        _logger.LogDebug("Resuming MonoBehaviour execution");
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