using System;
using System.Linq;

namespace Core.UnityHelpers.Types;

public abstract class EnumBase : Base
{
    public Type EnumType { get; protected set; }

    public override void Init(Type objType, UnityVersion version)
    {
        ObjType = objType;
        InitByUnityVersion(objType, version);
        InitEnumInternal(objType);
    }

    void InitEnumInternal(Type objType)
    {
        EnumType = GetEnumType();

        var variants = Enum.GetValues(objType).Cast<ulong>();
        var variantsString = Enum.GetValues(objType).Cast<string>();

        if (variants.Count() != Enum.GetValues(EnumType).Length)
        {
            throw new Exception("KeyCode variants count is not equal to KeyCodeTypes count");
        }

        Log.LogDebug($"Found {variants.Count()} variants, string variants: {string.Join(", ", variantsString)}");

        // check enum variant match
        for (int i = 0; i < variants.Count(); i++)
        {
            var variant = variants.ElementAt(i);
            var stringVariant = variantsString.ElementAt(i);

            if (Convert.ChangeType(variant, EnumType).ToString() != stringVariant)
            {
                throw new Exception("Enum variant mismatch");
            }
        }
    }

    protected abstract Type GetEnumType();

    public object To(object value)
    {
        return Convert.ChangeType((long)Convert.ChangeType(value, EnumType), ObjType);
    }

}
