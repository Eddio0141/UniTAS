using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UniTASPlugin.FixedUpdateSync;
using UniTASPlugin.GameEnvironment;
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

    public bool PendingRestart { get; private set; }

    public GameRestart(IVirtualEnvironmentFactory virtualEnvironmentFactory, ISyncFixedUpdate syncFixedUpdate,
        IUnityWrapper unityWrapper)
    {
        _virtualEnvironmentFactory = virtualEnvironmentFactory;
        _syncFixedUpdate = syncFixedUpdate;
        _unityWrapper = unityWrapper;

        StoreStaticFields();
    }

    // TODO remove this when we have a better way to handle static fields
    private void StoreStaticFields()
    {
        // ReSharper disable StringLiteralTypo
        var gameAssemblyNames = new[]
        {
            "Assembly-CSharp",
            "Assembly-CSharp-firstpass",
            "Assembly-UnityScript",
            "Assembly-UnityScript-firstpass"
        };
        // ReSharper restore StringLiteralTypo

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
                    if (!field.IsStatic || field.IsLiteral)
                    {
                        continue;
                    }

                    var value = field.GetValue(null);
                    // TODO remove hardcoded dependency
                    var valueClone = Helper.MakeDeepCopy(value, value.GetType());

                    // TODO remove hardcoded dependency
                    Plugin.Log.LogDebug($"Storing static field {type.FullName}.{field.Name} with value {valueClone}");
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
                var fieldClone = Helper.MakeDeepCopy(staticField.Value, staticField.Value.GetType());
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
            _unityWrapper.Object.FindObjectsOfType(_unityWrapper.MonoBehaviour.GetMonoBehaviourType());

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