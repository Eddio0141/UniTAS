using System.Collections.Generic;

namespace UniTAS.Patcher.Models.Serialization;

/// <summary>
/// Data that describes data for serialization
/// </summary>
public class SerializedData
{
    public string SourceClass { get; set; }
    public string SourceField { get; set; }

    public object Data { get; set; }

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
    public SerializedData(string sourceClass, string sourceField)
    {
        SourceClass = sourceClass;
        SourceField = sourceField;
        Fields = new();
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