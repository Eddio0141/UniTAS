using System;
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

        var variants = Enum.GetValues(objType).Cast<ulong>();
        var variantsString = Enum.GetValues(objType).Cast<string>();

        if (variants.Count() != Enum.GetValues(enumType).Length)
        {
            throw new Exception("KeyCode variants count is not equal to KeyCodeTypes count");
        }

        Log.LogDebug($"Found {variants.Count()} variants, string variants: {string.Join(", ", variantsString)}");

        // check enum variant match
        for (int i = 0; i < variants.Count(); i++)
        {
            var variant = variants.ElementAt(i);
            var stringVariant = variantsString.ElementAt(i);

            if (Convert.ChangeType(variant, enumType).ToString() != stringVariant)
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