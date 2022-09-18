using System;
using System.Collections.Generic;

namespace UniTASPlugin.UnityHooks.Helpers;

public abstract class BaseEnum<T> : Base<T>
{
    public static List<Type> EnumTypes { get; protected set; }
    protected static List<object> allVariants { get; set; }

    public string Value { get; set; }

    public BaseEnum(string value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    internal override void Init(Type objType, UnityVersion version)
    {
        ObjType = objType;
        InitByUnityVersion(objType, version);
        InitEnumInternal(objType);
    }

    void InitEnumInternal(Type objType)
    {
        EnumTypes = new();
        var enumTypesWithVersions = GetAllEnumTypes();

        var variants = Enum.GetValues(objType);
        allVariants = new List<object>();
        foreach (var enumTypeWithVersion in enumTypesWithVersions)
        {
            var version = enumTypeWithVersion.Key;
            var enumType = enumTypeWithVersion.Value;

            // don't allow enum version higher than current version
            if (version > Plugin.UnityVersion)
                break;

            var enumVariants = Enum.GetValues(enumType);
            EnumTypes.Add(enumType);

            foreach (var enumVariant in enumVariants)
            {
                allVariants.Add(enumVariant);
            }
        }

        if (variants.Length != allVariants.Count)
        {
            throw new Exception("Type variant count is not equal to dummy enum variant count");
        }

        // check enum if exists
        foreach (var variant in allVariants)
        {
            if (!Enum.IsDefined(ObjType, variant.ToString()))
            {
                throw new Exception($"Enum {variant} not found");
            }
        }
    }

    protected abstract Dictionary<UnityVersion, Type> GetAllEnumTypes();

    /// <summary>
    /// Converts dummy enum to original type.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    internal static object To(object value)
    {
        var valueVariants = Enum.GetValues(ObjType);
        var valueString = value.ToString();

        // get matching variant
        foreach (var variant in valueVariants)
        {
            if (variant.ToString() == valueString)
            {
                return variant;
            }
        }
        throw new InvalidOperationException("Enum variant not found");
    }

    /// <summary>
    /// Converts original enum to dummy enum type.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    internal static object From(object value)
    {
        var valueString = value.ToString();
        // get matching variant
        foreach (var variant in allVariants)
        {
            if (variant.ToString() == valueString)
            {
                return variant;
            }
        }
        throw new InvalidOperationException("Enum variant not found");
    }

    internal static bool IsDefined(object value)
    {
        var valueString = value.ToString();

        // get matching variant
        foreach (var variant in allVariants)
        {
            if (variant.ToString() == valueString)
            {
                return true;
            }
        }
        return false;
    }
}