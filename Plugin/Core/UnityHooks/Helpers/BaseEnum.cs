using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.UnityHooks.Helpers;

public abstract class BaseEnum<T, E> : Base<T>
{
    public static Type EnumType { get; protected set; }

    public override void Init(Type objType, UnityVersion version)
    {
        ObjType = objType;
        InitByUnityVersion(objType, version);
        InitEnumInternal(objType);
    }

    void InitEnumInternal(Type objType)
    {
        var enumType = typeof(E);

        var variants = Enum.GetValues(objType);
        var variantsWrappers = Enum.GetValues(enumType);

        if (variants.Length != Enum.GetValues(enumType).Length)
        {
            throw new Exception("Type variant count is not equal to dummy enum variant count");
        }

        // check enum variant match
        for (int i = 0; i < variants.Length; i++)
        {
            var variant = variants.GetValue(i);
            var variantWrapper = variantsWrappers.GetValue(i);

            if (variant.ToString() != variantWrapper.ToString())
            {
                throw new Exception("Enum variant mismatch");
            }
        }
    }

    /// <summary>
    /// Converts dummy enum to original type.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static object To(E value)
    {
        return Convert.ChangeType(Convert.ToInt64(value), ObjType);
    }

    /// <summary>
    /// Converts original enum to dummy enum type.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static E From(object value)
    {
        return (E)Convert.ChangeType((long)Convert.ChangeType(value, ObjType), typeof(E));
    }
}