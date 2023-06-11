using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Serialization;
using UniTAS.Patcher.Models.Serialization;
using UniTAS.Patcher.Services.Serialization;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.Implementations.Serialization;

[Singleton]
public class Serializer : ISerializer
{
    private readonly DataTypeSerializer[] _dataTypeSerializers;

    public Serializer(DataTypeSerializer[] dataTypeSerializers)
    {
        _dataTypeSerializers = dataTypeSerializers;
    }

    public TupleValue<IEnumerable<SerializedData>, IEnumerable<SerializedReferenceType>> SerializeStaticFields(
        string targetClass)
    {
        var target = AccessTools.TypeByName(targetClass);
        if (target == null)
        {
            throw new($"Could not find type {targetClass}");
        }

        var referenceData = new List<SerializedReferenceType>();
        return TupleValue.New(AccessTools.GetDeclaredFields(target).Where(x => x.IsStatic && !x.IsLiteral)
            .Select(x => SerializeField(targetClass, x, null, referenceData)), referenceData.AsEnumerable());
    }

    private SerializedData SerializeField(string className, FieldInfo field, object instance,
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
            object serializedReferenceData = null;
            if (_dataTypeSerializers.Any(x => x.Serialize(value, out serializedReferenceData)))
            {
                referenceData.Add(new(newReferenceId, new(newReferenceId, serializedReferenceData)));
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
        object serializedData = null;
        if (_dataTypeSerializers.Any(x => x.Serialize(value, out serializedData)))
        {
            return new(className, field.Name, serializedData);
        }

        // have to go through fields
        var fields2 = AccessTools.GetDeclaredFields(value.GetType()).Where(x => !x.IsStatic && !x.IsLiteral)
            .Select(x => SerializeField(null, x, value, referenceData));
        return new(className, field.Name, fields2.ToList());
    }
}