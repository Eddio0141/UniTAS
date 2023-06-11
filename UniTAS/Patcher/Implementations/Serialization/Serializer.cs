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
    public TupleValue<IEnumerable<SerializedData>, IEnumerable<SerializedReferenceType>> SerializeStaticFields(
        Type targetClass)
    {
        if (targetClass == null)
        {
            throw new ArgumentNullException(nameof(targetClass));
        }

        var referenceData = new List<SerializedReferenceType>();
        return TupleValue.New(AccessTools.GetDeclaredFields(targetClass).Where(x => x.IsStatic && !x.IsLiteral)
            .Select(x => SerializeField(targetClass.FullName, x, null, referenceData)), referenceData.AsEnumerable());
    }

    private static SerializedData SerializeField(string className, FieldInfo field, object instance,
        List<SerializedReferenceType> referenceData)
    {
        var value = field.GetValue(instance);

        // is it reference type?
        if (!field.FieldType.IsValueType)
        {
            if (value == null)
            {
                return new(className, field.Name, null);
            }

            // find if reference data already exists
            var foundReferenceIdIndex = referenceData.FindIndex(x => ReferenceEquals(x.Data.Data, value));
            if (foundReferenceIdIndex > -1)
            {
                return new(className, field.Name, referenceData[foundReferenceIdIndex].ReferenceId);
            }

            var newReferenceId = (uint)referenceData.Count;

            // can we serialize it?
            if (IsTypePrimitive(field.FieldType))
            {
                referenceData.Add(new(newReferenceId, new(newReferenceId, value)));
            }
            else
            {
                // we can't serialize it, so we need to handle
                var fields = AccessTools.GetDeclaredFields(value.GetType()).Where(x => !x.IsStatic && !x.IsLiteral)
                    .Select(x => SerializeField(null, x, value, referenceData));
                referenceData.Add(new(newReferenceId, new(newReferenceId, fields.ToList())));
            }

            return new(className, field.Name, newReferenceId);
        }

        // we can serialize the value type
        if (IsTypePrimitive(field.FieldType))
        {
            return new(className, field.Name, value);
        }

        // have to go through fields
        var fields2 = AccessTools.GetDeclaredFields(value.GetType()).Where(x => !x.IsStatic && !x.IsLiteral)
            .Select(x => SerializeField(null, x, value, referenceData));
        return new(className, field.Name, fields2.ToList());
    }

    private static bool IsTypePrimitive(Type type)
    {
        var isInSystemNamespace = type.Namespace?.StartsWith("System") ?? false;
        var hasSerializableAttribute = type.GetCustomAttributes(typeof(SerializableAttribute), true).Length > 0;
        return isInSystemNamespace && hasSerializableAttribute;
    }
}