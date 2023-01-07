using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Mono.Cecil;
using MonoMod.Utils;
using UniTASPlugin.FixedUpdateSync;
using UniTASPlugin.GameEnvironment;
using UniTASPlugin.Interfaces.StartEvent;
using UniTASPlugin.Interfaces.Update;
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

    private readonly List<StaticFieldStorage> _staticFields = new();

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
    }

    private void DestroyDontDestroyOnLoads()
    {
        var allObjects = _unityWrapper.Object.FindObjectsOfType(_unityWrapper.Object.ObjectType);
        foreach (var obj in allObjects)
        {
            if (obj is Plugin) continue;
            try
            {
                _logger.LogDebug($"Attempting destruction of {obj.GetType().FullName}");
                _unityWrapper.MonoBehaviour.StopAllCoroutines(obj);
                _unityWrapper.Object.DestroyImmediate(obj);
            }
            catch (Exception)
            {
                // ignored
                _logger.LogDebug("Failed to destroy");
            }
        }
    }

    private void StoreStaticFields()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var genericType = new List<Type>();

        foreach (var assembly in assemblies)
        {
            if (!gameAssemblyNames.Contains(assembly.GetName().Name))
            {
                continue;
            }

            var types = assembly.GetTypes();

            foreach (var type in types)
            {
                // if type has generic parameters
                if (type.IsGenericType)
                {
                    genericType.Add(type);
                    continue;
                }

                var fields = AccessTools.GetDeclaredFields(type).Where(x =>
                    x.IsStatic && !x.IsLiteral).ToArray();

                if (fields.Length == 0) continue;

                _logger.LogDebug(
                    $"type: {type.FullName} has {fields.Length} static fields, and static constructor: {type.TypeInitializer != null}");
                _staticFields.Add(new(fields, type.TypeInitializer));
            }
        }

        if (genericType.Count == 0) return;

        // find all types for each generic type
        var assemblyDefinitions = new List<AssemblyDefinition>();
        foreach (var assembly in assemblies)
        {
            try
            {
                assemblyDefinitions.Add(AssemblyDefinition.ReadAssembly(assembly.Location));
            }
            catch (Exception e)
            {
                // ignore
                _logger.LogWarning($"Failed to read assembly {assembly.FullName} with error: {e.Message}");
            }
        }

        // Link of generic types, and list of generic args (so a list of lists)
        var genericTypeUsedGenericTypes = new Dictionary<Type, List<List<Type>>>();

        // this is cursed but it works
        // iterates through all il instructions in all methods in all types in all assemblies to find usage of generic types
        foreach (var instruction in from assemblyDefinition in assemblyDefinitions
                 from moduleDefinition in assemblyDefinition.Modules
                 from typeDefinition in moduleDefinition.Types
                 from methodDefinition in typeDefinition.Methods
                 where methodDefinition.HasBody
                 from instruction in methodDefinition.Body.Instructions
                 select instruction)
        {
            if (instruction.Operand is not MemberReference
                {
                    DeclaringType: GenericInstanceType genericInstanceType
                }) continue;

            // if the generic type is not in the list of generic types we are looking for, skip
            var actualType = genericInstanceType.ElementType.ResolveReflection();
            var genericTypeFound = genericType.FirstOrDefault(x => x == actualType);

            if (genericTypeFound == null) continue;

            var allGenericTypes = genericInstanceType.GenericArguments
                .Select(x => x.ResolveReflection()).ToList();

            // add if allGenericTypes is not already in the list
            if (!genericTypeUsedGenericTypes.TryGetValue(genericTypeFound, out var list))
            {
                genericTypeUsedGenericTypes.Add(genericTypeFound, new() { allGenericTypes });
            }
            else
            {
                if (list.Any(x => x.SequenceEqual(allGenericTypes))) continue;
                genericTypeUsedGenericTypes[genericTypeFound].Add(allGenericTypes);
            }
        }

        // now we have a list of all generic types and their generic args
        // iterate through each generic type and go through the static field process again
        foreach (var typeUsedGenericTypes in genericTypeUsedGenericTypes)
        {
            var type = typeUsedGenericTypes.Key;
            var genericArgs = typeUsedGenericTypes.Value;

            foreach (var args in genericArgs)
            {
                var genericTypeDefinition = type.MakeGenericType(args.ToArray());
                var fields = AccessTools.GetDeclaredFields(genericTypeDefinition).Where(x =>
                    x.IsStatic && !x.IsLiteral).ToArray();

                if (fields.Length == 0) continue;

                _logger.LogDebug(
                    $"type: {genericTypeDefinition.FullName} has {fields.Length} static fields, and static constructor: {genericTypeDefinition.TypeInitializer != null}");
                _staticFields.Add(new(fields, genericTypeDefinition.TypeInitializer));
            }
        }
    }

    private void SetStaticFields()
    {
        _logger.LogDebug("setting static fields");
        foreach (var staticFields in _staticFields)
        {
            foreach (var field in staticFields.Fields)
            {
                _logger.LogDebug(
                    $"Setting static field: {field.DeclaringType?.FullName ?? "unknown_type"}.{field.Name} to null");
                field.SetValue(null, null);
            }

            _logger.LogDebug(
                $"Invoking static constructor for {staticFields.StaticConstructor?.DeclaringType?.FullName ?? "unknown_type"}");
            staticFields.StaticConstructor?.Invoke(null, null);
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
        StopScriptExecution();
        DestroyDontDestroyOnLoads();
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
        public FieldInfo[] Fields { get; }
        public ConstructorInfo StaticConstructor { get; }

        public StaticFieldStorage(FieldInfo[] fields, ConstructorInfo staticConstructor)
        {
            Fields = fields;
            StaticConstructor = staticConstructor;
        }
    }
}