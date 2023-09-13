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
    public IEnumerable<SerializedData> SerializeStaticFields(Type targetClass,
        List<(object, SerializedData)> references)
    {
        if (targetClass == null)
        {
            throw new ArgumentNullException(nameof(targetClass));
        }

        return AccessTools.GetDeclaredFields(targetClass).Where(x => x.IsStatic && !x.IsLiteral)
            .Select(x => SerializeField(targetClass.FullName, x, null, references));
    }

    private static SerializedData SerializeField(string className, FieldInfo field, object instance,
        List<(object, SerializedData)> references)
    {
        var value = field.GetValue(instance);

        if (value == null)
        {
            return new(className, field.Name, null as object);
        }

        // we can serialize the value type
        if (IsTypePrimitive(field.FieldType))
        {
            return new(className, field.Name, value);
        }

        // handle reference
        if (IsReferenceType(field.FieldType))
        {
            // check if we already serialized this reference
            var refIndex = references.FindIndex(x => ReferenceEquals(x.Item1, value));
            if (refIndex > -1)
            {
                var reference = references[refIndex];
                return new(className, field.Name, reference.Item2.SourceReference);
            }

            var newReferenceId = references.Count;

            if (IsTypePrimitive(field.FieldType))
            {
                references.Add((value, new(newReferenceId, value)));
                return new(className, field.Name, newReferenceId);
            }

            var fields = AccessTools.GetDeclaredFields(value.GetType()).Where(x => !x.IsStatic && !x.IsLiteral)
                .Select(x => SerializeField(null, x, value, references));

            // serialize reference
            references.Add((value, new(newReferenceId, fields)));
            return new(className, field.Name, newReferenceId);
        }

        // have to go through fields
        var fields2 = AccessTools.GetDeclaredFields(value.GetType()).Where(x => !x.IsStatic && !x.IsLiteral)
            .Select(x => SerializeField(null, x, value, references));
        return new(className, field.Name, fields2);
    }

    private static bool IsTypePrimitive(Type type)
    {
        var isInSystemNamespace = type.Namespace?.StartsWith("System") ?? false;
        var hasSerializableAttribute = type.GetCustomAttributes(typeof(SerializableAttribute), true).Length > 0;
        return isInSystemNamespace && hasSerializableAttribute;
    }

    private static bool IsReferenceType(Type type)
    {
        return type.IsClass && type != typeof(string);
    }
}