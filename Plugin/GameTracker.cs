using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UniTASPlugin.Exceptions;
using UniTASPlugin.VersionSafeWrapper;
using UnityEngine;
using Object = UnityEngine.Object;
// ReSharper disable StringLiteralTypo

namespace UniTASPlugin;

internal static class GameTracker
{
    internal static List<int> FirstObjIDs { get; } = new();
    internal static List<int> DontDestroyOnLoadIDs { get; private set; } = new();
    internal static Dictionary<Type, List<KeyValuePair<FieldInfo, object>>> InitialValues { get; private set; } = new();

    public static void Init()
    {
        var objs = Object.FindObjectsOfType(typeof(MonoBehaviour));
        foreach (var obj in objs)
        {
            var id = obj.GetInstanceID();
            if (DontDestroyOnLoadIDs.Contains(id))
            {
                FirstObjIDs.Add(id);
            }
        }

        // initial game state values
        var gameAssemblyNames = new[] {
            "Assembly-CSharp",
            "Assembly-CSharp-firstpass",
            "Assembly-UnityScript",
            "Assembly-UnityScript-firstpass"
        };
        // excludes all types
        var exclusionTypes = new[]
        {
            "Steamworks*",
            "UniRx*",
            // ignoring vr stuff
            "OVRSimpleJSON*",
            "OVRManager*",
            "OVRInput*",
            "OVRGearVrControllerTest*",
            "OVRHandTest*",
            "OVRResources*",
            "OVRPlugin*",
            "OVRMixedReality*",
            "OVROverlay*",
            "OVRHaptics*",
            "OVRBoundary*",
            "OVRPlatformMenu*",
            "OVRRaycaster*",
            "OVR.OpenVR.OpenVR*",
            "GvrAudio*",
            "GvrControllerInput*",
            "GvrKeyboard*",
            "GvrVideoPlayerTexture*",
            "GvrExecuteEventsExtension*",
            "GvrMathHelpers*",
            "GvrDaydreamApi*",
            "GvrPointerGraphicRaycaster*",
            "UnityEngine.EventSystems.OVRPhysicsRaycaster",
            //
            "DarkTonic.MasterAudio*",
            // HACK: need to look into how this one works, its an input manager
            "InControl*"
        };
        // game specific type exclusion
        var exclusionGameAndType = new Dictionary<string, List<string>>
        {
            { "It Steals", new List<string>
                {
                "SteamManager"
                }
            },
            { "Keep Talking and Nobody Explodes", new List<string>
                {
                /*
                "Oculus.Platform*",
                "DigitalOpus.MB.Core*",
                "LTGUI*",
                "LeanTween*",
                "LTDescr*",
                "LTRect*",
                "LeanTest*",
                "BindingsExample*",
                "I2.Loc*",
                */
            }
            }
        };
        // game specific field exclusion
        var exclusionGameAndField = new Dictionary<string, List<string>>
        {
            { "Keep Talking and Nobody Explodes", new List<string>
                {
                "InControl.TouchManager.OnSetup"
                }
            }
        };
        // TODO does System.Action affect stuff?

        var gameName = Helper.GameName();
        var gameAssemblies = AccessTools.AllAssemblies().Where(a => gameAssemblyNames.Contains(a.GetName().Name)).ToArray();
        var allGameTypes = gameAssemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => !exclusionTypes
            .Any(ex => t.FullName != null && (ex == t.FullName || ex.EndsWith("*") && t.FullName.StartsWith(ex.Remove(ex.Length - 1, 1)))));
        if (exclusionGameAndType.ContainsKey(gameName))
        {
            var excludeNames = exclusionGameAndType[gameName];
            allGameTypes =
                allGameTypes.Where(t => !excludeNames
                .Any(ex => t.FullName == ex || ex.EndsWith("*") && t.FullName.StartsWith(ex.Remove(ex.Length - 1, 1))));
        }
        var exclusionFields = new List<string>();
        if (exclusionGameAndField.ContainsKey(gameName))
            exclusionFields = exclusionGameAndField[gameName];

        /*
         * BUG: THIS TYPE CRASHES THE PLUGIN
        [RequiredByNativeCode(Optional = true, GenerateProxy = true)]
    	[NativeClass("Vector2f")]
	    [Il2CppEagerStaticClassConstruction]
	    public struct UnityEngine.Vector2 : IEquatable<Vector2>, IFormattable
        */

        //var fieldTypeIgnore = new List<System.Type>();
        foreach (var gameType in allGameTypes)
        {
            // get all static fields
            var fields = AccessTools.GetDeclaredFields(gameType);

            foreach (var field in fields)
            {
                var fieldName = $"{gameType.FullName}.{field.Name}";
                if (!field.IsStatic || field.IsLiteral || field.IsInitOnly)
                    continue;

                // check instance field
                var fieldType = field.FieldType;
                if (fieldType == gameType)
                {
                    Plugin.Log.LogDebug($"Detected instance field: {fieldName}, skipping");
                    continue;
                }
                // check field exclusion
                if (exclusionFields.Contains(fieldName))
                {
                    Plugin.Log.LogDebug($"Exclusion fields contains {fieldName}");
                    continue;
                }
                // check field type exclusion
                /*
                if (fieldTypeIgnore.Contains(fieldType))
                {
                    Plugin.Instance.Log.LogDebug($"Ignoring field {fieldName} for safety");
                    continue;
                }
                */

                if (!InitialValues.ContainsKey(gameType))
                    InitialValues.Add(gameType, new());

                // handling some types by hand since they crash AccessTools.MakeDeepCopy
                // HACK make my own fixed version of this method
                object objClone = null;
                var failedClone = false;
                // we can't get value of an instance field, it crashes everything
                object fieldValue = null;
                try
                {
                    fieldValue = field.GetValue(null);
                }
                catch (Exception ex)
                {
                    Plugin.Log.LogWarning($"failed to get field value for {fieldName}, ex: {ex}");
                    continue;
                }
                {
                    var fieldValueString = fieldValue == null ? "null" : fieldValue.ToString();
                    Plugin.Log.LogDebug($"cloning field {fieldName} with value {fieldValueString}");
                    //System.Threading.Thread.Sleep(50);

                    try
                    {
                        if (fieldValue == null || !typeof(IEnumerable).IsAssignableFrom(fieldType))
                        {
                            objClone = Helper.MakeDeepCopy(fieldValue, fieldType);
                        }
                        /*
                        else if (fieldValue == null)
                        {
                            Plugin.Instance.Log.LogDebug("field is null, skipping for safety");
                            continue;
                        }
                        */
                        else
                        {
                            Plugin.Log.LogDebug("skipping collection convertion for now");
                            continue;
                            /*
                            // manually clone field if its a collection
                            var fieldValueAsCollection = ((System.Collections.IEnumerable)fieldValue).Cast<object>();
                            var clonedCollection = new List<object>();
                            foreach (var value in fieldValueAsCollection)
                            {
                                clonedCollection.Add(Helper.MakeDeepCopy(value, value.GetType()));
                            }

                            // convert List to appropriate type if needed to
                            switch (fieldType.GetGenericTypeDefinition().FullName)
                            {
                                case "System.Collections.Generic.Stack`1":
                                    {
                                        var stackValue = new Stack<object>(clonedCollection);
                                        objClone = stackValue;
                                    }
                                    break;
                                default:
                                    {
                                        objClone = clonedCollection;
                                        Plugin.Instance.Log.LogDebug($"field type: {fieldType.GetGenericTypeDefinition().FullName}");
                                    }
                                    break;
                            }
                            */
                        }
                    }
                    catch (DeepCopyMaxRecursion)
                    {
                        failedClone = true;
                        Plugin.Log.LogWarning($"max recursion reached, excluding field type {fieldType} from deep copy");
                        //fieldTypeIgnore.Add(fieldType);
                    }
                    catch (Exception ex)
                    {
                        failedClone = true;
                        Plugin.Log.LogWarning($"failed to clone field, ex: {ex.Message}");
                    }
                }
                if (!failedClone)
                    InitialValues[gameType].Add(new(field, objClone));
                else
                    failedClone = false;
            }
        }
    }

    public static void LateUpdate()
    {
        foreach (var scene in asyncSceneLoads)
        {
            Plugin.Log.LogDebug($"force loading scene, name: {scene.sceneName} {scene.sceneBuildIndex}");
            SceneHelper.LoadSceneAsyncNameIndexInternal(scene.sceneName, scene.sceneBuildIndex, scene.parameters, scene.isAdditive, true);
        }
        asyncSceneLoads.Clear();
    }

    private struct AsyncSceneLoadData
    {
        public string sceneName;
        public int sceneBuildIndex;
        public object parameters;
        public bool? isAdditive;
        public ulong UID;

        public AsyncSceneLoadData(string sceneName, int sceneBuildIndex, object parameters, bool? isAdditive, AsyncOperationWrap wrap)
        {
            this.sceneName = sceneName;
            this.sceneBuildIndex = sceneBuildIndex;
            this.parameters = parameters;
            this.isAdditive = isAdditive;
            UID = wrap.UID;
        }
    }

    private static readonly List<AsyncSceneLoadData> asyncSceneLoads = new();
    private static readonly List<AsyncSceneLoadData> asyncSceneLoadsStall = new();

    public static void AsyncSceneLoad(string sceneName, int sceneBuildIndex, object parameters, bool? isAdditive, AsyncOperationWrap wrap)
    {
        asyncSceneLoads.Add(new(sceneName, sceneBuildIndex, parameters, isAdditive, wrap));
    }

    public static void AllowSceneActivation(bool allow, AsyncOperation instance)
    {
        var wrap = new AsyncOperationWrap(instance);
        var uid = wrap.UID;
        Plugin.Log.LogDebug($"allow scene activation {allow} for UID {uid}");
        if (wrap.InstantiatedByUnity)
        {
            Plugin.Log.LogError("AsyncOperation UID is 0, this should not happen");
            return;
        }

        if (allow)
        {
            var sceneToLoadIndex = asyncSceneLoadsStall.FindIndex(s => s.UID == uid);
            if (sceneToLoadIndex < 0)
                return;
            var sceneToLoad = asyncSceneLoadsStall[sceneToLoadIndex];
            asyncSceneLoadsStall.RemoveAt(sceneToLoadIndex);
            SceneHelper.LoadSceneAsyncNameIndexInternal(sceneToLoad.sceneName, sceneToLoad.sceneBuildIndex, sceneToLoad.parameters, sceneToLoad.isAdditive, true);
            Plugin.Log.LogDebug($"force loading scene, name: {sceneToLoad.sceneName} build index: {sceneToLoad.sceneBuildIndex}");
        }
        else
        {
            var asyncSceneLoadsIndex = asyncSceneLoads.FindIndex(s => s.UID == uid);
            if (asyncSceneLoadsIndex < 0)
                return;
            var scene = asyncSceneLoads[asyncSceneLoadsIndex];
            asyncSceneLoads.RemoveAt(asyncSceneLoadsIndex);
            asyncSceneLoadsStall.Add(scene);
            Plugin.Log.LogDebug("Added scene to stall list");
        }
    }

    public static void AsyncOperationFinalize(ulong uid)
    {
        var removeIndex = asyncSceneLoadsStall.FindIndex(a => a.UID == uid);
        if (removeIndex < 0)
            return;
        asyncSceneLoadsStall.RemoveAt(removeIndex);
    }

    public static bool GetSceneActivation(AsyncOperation instance)
    {
        var uid = new AsyncOperationWrap(instance).UID;
        return uid != 0 && asyncSceneLoadsStall.Any(a => a.UID == uid);
    }

    public static bool IsStallingInstance(AsyncOperation instance)
    {
        var uid = new AsyncOperationWrap(instance).UID;
        return uid != 0 && asyncSceneLoadsStall.Any(x => x.UID == uid);
    }

    public static void DontDestroyOnLoadCall(Object @object)
    {
        if (@object == null)
            return;
        var id = @object.GetInstanceID();
        if (DontDestroyOnLoadIDs.Contains(id))
            return;
        DontDestroyOnLoadIDs.Add(id);
    }

    public static void DestroyObject(Object @object)
    {
        if (@object == null)
            return;
        _ = DontDestroyOnLoadIDs.Remove(@object.GetInstanceID());
    }
}