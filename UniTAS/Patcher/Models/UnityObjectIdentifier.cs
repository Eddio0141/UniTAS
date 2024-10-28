using System;
using System.Linq;
using Newtonsoft.Json;
using UniTAS.Patcher.Services;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UniTAS.Patcher.Models;

/// <summary>
/// Attempts to identify a unity object created at runtime
/// </summary>
public class UnityObjectIdentifier
{
    private bool Equals(UnityObjectIdentifier other)
    {
        return _objectType == other._objectType && _id == other._id && Equals(Parent, other.Parent) &&
               string.Equals(Name, other.Name, StringComparison.InvariantCulture);
    }

    public override string ToString()
    {
        return $"_id: {_id}, Name: {Name}, Parent: {Parent}";
    }

    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((UnityObjectIdentifier)obj);
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(_objectType);
        hashCode.Add(_id);
        hashCode.Add(Parent);
        hashCode.Add(Name, StringComparer.InvariantCulture);
        return hashCode.ToHashCode();
    }

    [JsonConstructor]
    private UnityObjectIdentifier()
    {
    }

    /// <summary>
    /// Initialise from object
    /// </summary>
    public UnityObjectIdentifier(Object o, IUnityObjectIdentifierFactory unityObjectIdentifierFactory)
    {
        _objectType = o.GetType();

        var instanceId = o.GetInstanceID();
        if (instanceId > 0)
        {
            _id = instanceId;
            return;
        }

        // runtime object
        Name = o.name;

        var t = o switch
        {
            GameObject gameObject => gameObject.transform,
            Transform transform => transform,
            Component component => component.transform,
            _ => null
        };

        var parent = t?.parent?.gameObject;
        if (parent == null) return;
        Parent = unityObjectIdentifierFactory.NewUnityObjectIdentifier(parent);
    }

    [JsonProperty] private readonly Type _objectType;

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
    [JsonProperty]
    private UnityObjectIdentifier Parent { get; }

    [JsonProperty] private string Name { get; }

    #endregion

    /// <summary>
    /// Finds runtime object
    /// </summary>
    /// <param name="allObjects">Objects to search from, otherwise it will automatically search</param>
    /// <param name="allObjectsWithType">Objects with matching type as identifier, otherwise it will search automatically</param>
    public Object FindObject(Object[] allObjects = null, Object[] allObjectsWithType = null)
    {
        allObjects ??= Resources.FindObjectsOfTypeAll(_objectType);
        allObjectsWithType ??= allObjects.Where(x => x.GetType() == _objectType).ToArray();

        if (_id.HasValue)
        {
            return allObjectsWithType.FirstOrDefault(o => o.GetInstanceID() == _id.Value);
        }

        // runtime find
        if (allObjectsWithType.Length == 1) return allObjectsWithType[0]; // how lucky!

        var filteredObjs = allObjectsWithType.Where(o => o.name == Name).ToArray();
        if (filteredObjs.Length == 1) return filteredObjs[0];

        var parent = Parent?.FindObject(allObjects, allObjectsWithType) as GameObject;
        if (parent == null) return null;
        var filteredChildren = parent.GetComponentsInChildren(_objectType, true)
            .Where(t => filteredObjs.Any(o => o == t))
            .ToArray();

        if (filteredChildren.Length == 1) return filteredChildren[0];
        return null;
    }
}