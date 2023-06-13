using System.Collections.Generic;
using System.Linq;

namespace UniTAS.Patcher.Models.Serialization;

/// <summary>
/// Data that describes data for serialization
/// </summary>
public class SerializedData
{
    public string SourceClass { get; set; }
    public string SourceField { get; set; }
    public int? SourceReference { get; set; }

    public object Data { get; set; }
    public int? ReferenceData { get; set; }

    /// <summary>
    /// If this data can't be serialized or isn't reference type, then this will be populated with the data that can be serialized
    /// </summary>
    public List<SerializedData> Fields { get; set; }

    private SerializedData()
    {
    }

    /// <summary>
    /// For data that isn't primitive or handled by the serializer
    /// </summary>
    public SerializedData(string sourceClass, string sourceField, IEnumerable<SerializedData> fields)
    {
        SourceClass = sourceClass;
        SourceField = sourceField;
        Fields = fields.ToList();
    }

    /// <summary>
    /// For data that is referencing some other data
    /// </summary>
    public SerializedData(string sourceClass, string sourceField, int? referenceData)
    {
        SourceClass = sourceClass;
        SourceField = sourceField;
        ReferenceData = referenceData;
    }

    /// <summary>
    /// For data that can be referenced
    /// </summary>
    public SerializedData(int? sourceReference, object data)
    {
        SourceReference = sourceReference;
        Data = data;
    }

    public SerializedData(int? sourceReference, IEnumerable<SerializedData> fields)
    {
        SourceReference = sourceReference;
        Fields = fields.ToList();
    }

    /// <summary>
    /// For data that can be serialized
    /// </summary>
    public SerializedData(string sourceClass, string sourceField, object data)
    {
        SourceClass = sourceClass;
        SourceField = sourceField;
        Data = data;
    }
}