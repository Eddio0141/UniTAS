using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Newtonsoft.Json;
using UniTAS.Patcher.Extensions;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.UnitySafeWrappers.Wrappers;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UniTAS.Patcher.Models;

/// <summary>
/// Attempts to identify a unity object created at runtime
/// </summary>
public class UnityObjectIdentifier
{
    public override string ToString()
    {
        return
            $"{nameof(_id)}: {_id}, {nameof(_name)}: {_name}, {nameof(_objectType)}: {_objectType}" +
            $", {nameof(_foundScene)}: {_foundScene}, {nameof(_componentTypes)}: {_componentTypes.Join()}" +
            $", {nameof(_unityObjectType)}: {_unityObjectType}, {nameof(_parent)}: {_parent}";
    }

    [JsonConstructor]
    private UnityObjectIdentifier()
    {
    }

    /// <summary>
    /// Initialise from object
    /// </summary>
    public UnityObjectIdentifier(Object o, IUnityObjectIdentifierFactory unityObjectIdentifierFactory,
        ISceneManagerWrapper iSceneManagerWrapper)
    {
        _objectType = o.GetType();

        var instanceId = o.GetInstanceID();
        if (instanceId > 0)
        {
            _id = instanceId;
        }

        // runtime object info
        _name = o.name;

        var (t, objType) = o switch
        {
            GameObject gameObject => (gameObject.transform, UnityObjectType.GameObject),
            Transform transform => (transform, UnityObjectType.Transform),
            Component component => (component.transform, UnityObjectType.Component),
            _ => (null, UnityObjectType.Other),
        };

        _unityObjectType = objType;

        _componentTypes = t?.GetComponents<Component>()?.Select(x => x.GetType()).ToArray() ?? [];

        _foundScene = iSceneManagerWrapper.ActiveSceneIndex;

        var parent = t?.parent?.gameObject;
        if (parent == null) return;
        _parent = unityObjectIdentifierFactory.NewUnityObjectIdentifier(parent);
    }

    [JsonProperty] private readonly Type _objectType;
    [JsonProperty] private readonly UnityObjectType _unityObjectType;

    #region DiskObject

    /// <summary>
    /// Instance ID if the ID is positive
    /// </summary>
    [JsonProperty] private readonly int? _id;

    #endregion

    #region RuntimeObject

    /// <summary>
    /// A parent object, if any
    /// It will point to a GameObject
    /// </summary>
    [JsonProperty] private readonly UnityObjectIdentifier _parent;

    [JsonProperty] private readonly string _name;

    [JsonProperty] private readonly Type[] _componentTypes;

    [JsonProperty] private readonly int _foundScene;

    #endregion

    /// <summary>
    /// Finds runtime object
    /// </summary>
    /// <param name="searchSettings">Search settings to loosen or strict search</param>
    /// <param name="iSceneManagerWrapper"></param>
    /// <param name="alreadyTrackedObjects">Array of already tracked objects</param>
    /// <param name="allObjects">Objects to search from, otherwise it will automatically search</param>
    /// <param name="allObjectsWithType">Objects with matching type as identifier, otherwise it will search automatically</param>
    public Object FindObject(SearchSettings searchSettings, ISceneManagerWrapper iSceneManagerWrapper,
        HashSet<Object> alreadyTrackedObjects, Object[] allObjects = null,
        Object[] allObjectsWithType = null)
    {
        if (searchSettings.SceneMatch && _foundScene != iSceneManagerWrapper.ActiveSceneIndex) return null;

        allObjects ??= Resources.FindObjectsOfTypeAll(_objectType);
        allObjectsWithType ??= allObjects.Where(x => x.GetType() == _objectType).ToArray();

        if (_id.HasValue && searchSettings.IdSearchType is IdSearchType.Match)
            return allObjectsWithType.FirstOrDefault(o => o.GetInstanceID() == _id.Value);

        // runtime find
        if (allObjectsWithType.Length == 1)
            return allObjectsWithType[0]; // how lucky!

        var filteredObjs = allObjectsWithType;
        // filter by active state
        if (searchSettings.Active.HasValue)
        {
            var active = searchSettings.Active.Value;
            filteredObjs = filteredObjs.Where(o =>
            {
                var t = GetTransform(o);
                if (ActiveInHierarchy != null)
                    return active == ActiveInHierarchy(t.gameObject);
                return active == t.gameObject.active;
            }).ToArray();
        }

        if (searchSettings.NameMatch)
        {
            filteredObjs = filteredObjs.Where(o => o.name == _name).ToArray();
            if (filteredObjs.Length == 1)
                return filteredObjs[0];
        }

        if (searchSettings.ParentMatch && _parent != null)
        {
            var parentFindSettings = searchSettings;
            // must fail match because it could mess up parent matching completely
            parentFindSettings.MultipleMatchHandle = MultipleMatchHandle.FailMatch;
            var parent =
                _parent.FindObject(parentFindSettings, iSceneManagerWrapper, alreadyTrackedObjects, allObjects) as GameObject;
            parent ??= _parent.FindObject(new(), iSceneManagerWrapper, alreadyTrackedObjects, allObjects) as GameObject;
            if (parent == null) return null;
            var parentTransform = parent.transform;
            var childCount = parentTransform.childCount;
            var childrenSearch = new HashSet<Object>();
            for (var i = 0; i < childCount; i++)
            {
                var child = parentTransform.GetChild(i);
                var childFixed = _unityObjectType switch
                {
                    UnityObjectType.GameObject => [child.gameObject],
                    UnityObjectType.Transform => [child],
                    UnityObjectType.Component => child.GetComponents(_objectType).Cast<Object>(),
                    UnityObjectType.Other => throw new InvalidOperationException(
                        "Considering there's a parent, this is not possible"),
                    _ => throw new ArgumentOutOfRangeException()
                };
                childrenSearch.AddRange(childFixed);
            }

            filteredObjs = filteredObjs.Where(o => childrenSearch.Remove(o)).ToArray();
            if (filteredObjs.Length == 1)
                return filteredObjs[0];
        }

        if (searchSettings.ComponentMatch && _unityObjectType != UnityObjectType.Other)
        {
            var componentTypes = filteredObjs
                .Select(x => _unityObjectType switch
                {
                    UnityObjectType.GameObject => ((GameObject)x).transform,
                    UnityObjectType.Transform => ((Transform)x).transform,
                    UnityObjectType.Component => ((Component)x).transform,
                    UnityObjectType.Other => throw new InvalidOperationException(),
                    _ => throw new ArgumentOutOfRangeException()
                }).Select(x => x.GetComponents<Component>().Select(c => c.GetType())).ToArray();

            var removeIndexes = new List<int>();
            for (var i = 0; i < componentTypes.Length; i++)
            {
                var types = componentTypes[i];
                var searchTypes = new HashSet<Type>(_componentTypes);
                foreach (var t in types)
                {
                    if (!searchTypes.Remove(t)) break;
                }

                if (searchTypes.Count > 0)
                    removeIndexes.Add(i);
            }

            // stupid
            removeIndexes.Reverse();
            var filteredObjsList = filteredObjs.ToList();
            foreach (var i in removeIndexes)
            {
                filteredObjsList.RemoveAt(i);
            }

            filteredObjs = filteredObjsList.ToArray();

            if (filteredObjs.Length == 1)
                return filteredObjs[0];
        }

        if (_id.HasValue && searchSettings.IdSearchType is IdSearchType.Fallback)
        {
            var id = _id.Value;
            var obj = allObjectsWithType.FirstOrDefault(o => o.GetInstanceID() == id);
            if (obj != null)
                return obj;
        }

        if (searchSettings.MultipleMatchHandle is MultipleMatchHandle.FirstUntracked)
        {
            foreach (var obj in filteredObjs)
            {
                if (alreadyTrackedObjects.Contains(obj)) continue;
                return obj;
            }
        }

        return null;
    }

    private Transform GetTransform(Object o)
    {
        if (o == null) return null;
        return _unityObjectType switch
        {
            UnityObjectType.GameObject => ((GameObject)o).transform,
            UnityObjectType.Transform => (Transform)o,
            UnityObjectType.Component => ((Component)o).transform,
            UnityObjectType.Other => null,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private static readonly Func<GameObject, bool> ActiveInHierarchy;

    static UnityObjectIdentifier()
    {
        var activeInHierarchy = AccessTools.PropertyGetter(typeof(GameObject), "activeInHierarchy");
        if (activeInHierarchy != null)
            ActiveInHierarchy = AccessTools.MethodDelegate<Func<GameObject, bool>>(activeInHierarchy);
    }

    private enum UnityObjectType
    {
        GameObject,
        Transform,
        Component,
        Other
    }

    /// <summary>
    /// How to search for the object
    /// </summary>
    public struct SearchSettings
    {
        public IdSearchType IdSearchType = IdSearchType.Fallback;

        /// <summary>
        /// How to handle multiple of the same results at the end
        /// </summary>
        public MultipleMatchHandle MultipleMatchHandle = MultipleMatchHandle.FirstUntracked;

        public bool? Active;

        public bool NameMatch = true;
        public bool ParentMatch = true;
        public bool ComponentMatch = true;
        public bool SceneMatch = false;

        public SearchSettings()
        {
        }
    }

    public enum IdSearchType
    {
        Match,
        Fallback,
        Disabled
    }

    public enum MultipleMatchHandle
    {
        FirstUntracked,
        FailMatch
    }
}