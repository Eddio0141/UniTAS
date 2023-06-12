using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Models.Serialization;
using UniTAS.Patcher.Services.Serialization;

namespace UniTAS.Patcher.Implementations.Serialization;

[Singleton]
public class Serializer : ISerializer
{
    public IEnumerable<SerializedData> SerializeStaticFields(Type targetClass)
    {
        if (targetClass == null)
        {
            throw new ArgumentNullException(nameof(targetClass));
        }

        return AccessTools.GetDeclaredFields(targetClass).Where(x => x.IsStatic && !x.IsLiteral)
            .Select(x => SerializeField(targetClass.FullName, x, null));
    }

    private static SerializedData SerializeField(string className, FieldInfo field, object instance)
    {
        var value = field.GetValue(instance);

        if (value == null)
        {
            return new(className, field.Name, null);
        }

        // we can serialize the value type
        if (IsTypePrimitive(field.FieldType))
        {
            return new(className, field.Name, value);
        }

        // have to go through fields
        var fields2 = AccessTools.GetDeclaredFields(value.GetType()).Where(x => !x.IsStatic && !x.IsLiteral)
            .Select(x => SerializeField(null, x, value));
        return new(className, field.Name, fields2.ToList());
    }

    private static bool IsTypePrimitive(Type type)
    {
        var isInSystemNamespace = type.Namespace?.StartsWith("System") ?? false;
        var hasSerializableAttribute = type.GetCustomAttributes(typeof(SerializableAttribute), true).Length > 0;
        return isInSystemNamespace && hasSerializableAttribute;
    }
}