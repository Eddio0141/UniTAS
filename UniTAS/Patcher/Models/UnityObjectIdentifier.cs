using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Newtonsoft.Json;
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
        ISceneWrapper sceneWrapper)
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

        _foundScene = sceneWrapper.ActiveSceneIndex;

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
    /// Either an ID of an object existing in disk, or another runtime object
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
    /// <param name="sceneWrapper"></param>
    /// <param name="allObjects">Objects to search from, otherwise it will automatically search</param>
    /// <param name="allObjectsWithType">Objects with matching type as identifier, otherwise it will search automatically</param>
    public Object FindObject(SearchSettings searchSettings, ISceneWrapper sceneWrapper, Object[] allObjects = null,
        Object[] allObjectsWithType = null)
    {
        if (searchSettings.SceneMatch && _foundScene != sceneWrapper.ActiveSceneIndex) return null;

        allObjects ??= Resources.FindObjectsOfTypeAll(_objectType);
        allObjectsWithType ??= allObjects.Where(x => x.GetType() == _objectType).ToArray();

        if (_id.HasValue && searchSettings.IdSearchType is IdSearchType.Match)
        {
            return allObjectsWithType.FirstOrDefault(o => o.GetInstanceID() == _id.Value);
        }

        // runtime find
        if (allObjectsWithType.Length == 1) return allObjectsWithType[0]; // how lucky!

        var filteredObjs = allObjectsWithType;
        if (searchSettings.NameMatch)
        {
            filteredObjs = filteredObjs.Where(o => o.name == _name).ToArray();
            if (filteredObjs.Length == 1) return filteredObjs[0];
        }

        if (searchSettings.ParentMatch)
        {
            var parent =
                _parent?.FindObject(searchSettings, sceneWrapper, allObjects, allObjectsWithType) as GameObject;
            if (parent == null) return null;
            filteredObjs = parent.GetComponentsInChildren(_objectType, true)
                .Where(t => filteredObjs.Any(o => o == t)).Select(Object (t) => t)
                .ToArray();

            if (filteredObjs.Length == 1) return filteredObjs[0];
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
                var searchTypes = _componentTypes.ToList();
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

            if (filteredObjs.Length == 1) return filteredObjs[0];
        }

        if (_id.HasValue && searchSettings.IdSearchType is IdSearchType.Fallback)
        {
            return allObjectsWithType.FirstOrDefault(o => o.GetInstanceID() == _id.Value);
        }

        return null;
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
        public IdSearchType IdSearchType;
        public bool NameMatch;
        public bool ParentMatch;
        public bool ComponentMatch;
        public bool SceneMatch;
    }

    public enum IdSearchType
    {
        Match,
        Fallback,
        Disabled
    }
}