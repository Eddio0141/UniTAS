using System;
using System.Collections.Generic;

namespace UniTAS.Patcher.Models.Serialization;

/// <summary>
/// Data that describes data for serialization
/// </summary>
[Serializable]
public class SerializedData
{
    public string SourceClass { get; }
    public string SourceField { get; }

    /// <summary>
    /// The reference id of the data that this data references
    /// </summary>
    public uint SourceReferenceId { get; }

    /// <summary>
    /// Serialized data. If this is null, then this data is a reference to another data with <see cref="ReferenceId"/>
    /// </summary>
    public object Data { get; }

    /// <summary>
    /// If this data is a reference to another data but that data is null, then this will be true
    /// </summary>
    public bool IsNullReferenceData { get; }

    /// <summary>
    /// If this data can't be serialized or isn't reference type, then this will be populated with the data that can be serialized
    /// </summary>
    public List<SerializedData> Fields { get; }

    /// <summary>
    /// 
    /// </summary>
    public uint? ReferenceId { get; }

    /// <summary>
    /// For data that isn't primitive or handled by the serializer
    /// </summary>
    public SerializedData(string sourceClass, string sourceField)
    {
        SourceClass = sourceClass;
        SourceField = sourceField;
        Fields = new();
    }

    /// <summary>
    /// For data that has reference to reference data
    /// </summary>
    /// <summary>
    /// For data that has reference to reference data
    /// </summary>
    public SerializedData(string sourceClass, string sourceField, uint referenceId)
    {
        SourceClass = sourceClass;
        SourceField = sourceField;
        ReferenceId = referenceId;
    }

    public SerializedData(uint sourceReferenceId, object data)
    {
        SourceReferenceId = sourceReferenceId;
        Data = data;
        IsNullReferenceData = data == null;
    }

    public SerializedData(uint sourceReferenceId, List<SerializedData> fields)
    {
        SourceReferenceId = sourceReferenceId;
        Fields = fields;
    }

    /// <summary>
    /// For data that can be serialized
    /// </summary>
    public SerializedData(string sourceClass, string sourceField, object data)
    {
        SourceClass = sourceClass;
        SourceField = sourceField;
        Data = data;
        IsNullReferenceData = data == null;
    }
}