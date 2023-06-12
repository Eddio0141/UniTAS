using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Models.Serialization;
using UniTAS.Patcher.Services.Serialization;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.Implementations.Serialization;

[Singleton]
public class Serializer : ISerializer
{
    public TupleValue<IEnumerable<SerializedData>, IEnumerable<SerializedData>> SerializeStaticFields(Type targetClass)
    {
        if (targetClass == null)
        {
            throw new ArgumentNullException(nameof(targetClass));
        }

        var references = new List<SerializedData>();
        return new(AccessTools.GetDeclaredFields(targetClass).Where(x => x.IsStatic && !x.IsLiteral)
            .Select(x => SerializeField(targetClass.FullName, x, null, references)), references);
    }

    private static SerializedData SerializeField(string className, FieldInfo field, object instance,
        List<SerializedData> references)
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
            var reference = references.FirstOrDefault(x => x.Data == value);
            if (reference != null)
            {
                return new(className, field.Name, reference.ReferenceData);
            }

            var newReferenceId = references.Count;

            if (IsTypePrimitive(field.FieldType))
            {
                return new(newReferenceId, value);
            }

            var fields = AccessTools.GetDeclaredFields(value.GetType()).Where(x => !x.IsStatic && !x.IsLiteral)
                .Select(x => SerializeField(null, x, value, references));

            // serialize reference
            var refData = new SerializedData(newReferenceId, fields);
            references.Add(refData);
            return refData;
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