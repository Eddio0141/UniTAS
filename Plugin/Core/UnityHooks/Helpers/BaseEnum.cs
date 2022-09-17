using System;
using System.Collections.Generic;

namespace Core.UnityHooks.Helpers;

public abstract class BaseEnum<T> : Base<T>
{
    public static List<Type> EnumTypes { get; protected set; }

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
        var variantsWrappers = new List<Type>();
        foreach (var enumTypeWithVersion in enumTypesWithVersions)
        {
            var version = enumTypeWithVersion.Key;
            var enumType = enumTypeWithVersion.Value;

            // don't allow enum version higher than current version
            if (version > PluginInfo.UnityVersion)
            {
                break;
            }

            EnumTypes.Add(enumType);

            var enumVariants = Enum.GetValues(enumType);
            foreach (var enumVariant in enumVariants)
            {
                variantsWrappers.Add((Type)enumVariant);
            }
        }

        if (variants.Length != variantsWrappers.Count)
        {
            throw new Exception("Type variant count is not equal to dummy enum variant count");
        }

        // check enum variant match
        for (int i = 0; i < variants.Length; i++)
        {
            var variant = variants.GetValue(i);
            var variantWrapper = variantsWrappers[i];

            if (variant.ToString() != variantWrapper.ToString())
            {
                throw new Exception("Enum variant mismatch");
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
        foreach (var enumType in EnumTypes)
        {
            var valueVariants = Enum.GetValues(enumType);
            // get matching variant
            foreach (var variant in valueVariants)
            {
                if (variant.ToString() == valueString)
                {
                    return variant;
                }
            }
        }
        throw new InvalidOperationException("Enum variant not found");
    }
}