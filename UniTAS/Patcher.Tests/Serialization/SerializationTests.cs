using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using UniTAS.Patcher.Models.Serialization;
using UniTAS.Patcher.Models.Utils;
using UniTAS.Patcher.Services.Serialization;

namespace Patcher.Tests.Serialization;

public class SerializationTests
{
    [Fact]
    public void SerializeData()
    {
        var data = new SerializedData("TestClass", "TestField", 0);
        var xmlSerializer = new XmlSerializer(typeof(SerializedData));
        var writer = new StringWriter();
        xmlSerializer.Serialize(writer, data);

        var reader = new StringReader(writer.ToString());

        var deserializedData = (SerializedData)xmlSerializer.Deserialize(reader)!;

        Assert.Equal("TestClass", deserializedData.SourceClass);
        Assert.Equal("TestField", deserializedData.SourceField);
    }

    [Fact]
    public void TestClassWithInts()
    {
        var kernel = KernelUtils.Init();
        var serializer = kernel.GetInstance<ISerializer>();

        var serializedData = serializer.SerializeStaticFields(typeof(SerializationUtils.TestClassWithInts), new())
            .ToList();

        TestClassWithIntsInner(serializedData);

        var stream = new MemoryStream();
        var xmlSerializer = new XmlSerializer(typeof(List<SerializedData>));
        xmlSerializer.Serialize(stream, serializedData);

        stream.Position = 0;

        var deserializedData = (List<SerializedData>)xmlSerializer.Deserialize(stream)!;

        TestClassWithIntsInner(deserializedData);
    }

    private static void TestClassWithIntsInner(IReadOnlyList<SerializedData> serializedData)
    {
        Assert.Equal(2, serializedData.Count);
        Assert.Equal(typeof(SerializationUtils.TestClassWithInts).FullName, serializedData[0].SourceClass);
        Assert.Equal(nameof(SerializationUtils.TestClassWithInts.Int1), serializedData[0].SourceField);
        Assert.Equal(1, serializedData[0].Data);
        Assert.Equal("_int2", serializedData[1].SourceField);
        Assert.Equal(2, serializedData[1].Data);
    }

    [Fact]
    public void InstanceLoop()
    {
        var kernel = KernelUtils.Init();
        var serializer = kernel.GetInstance<ISerializer>();

        var serializedData = serializer
            .SerializeStaticFields(typeof(SerializationUtils.InstanceLoop), new()).ToList();
        var stream = new MemoryStream();
        var xmlSerializer = new XmlSerializer(typeof(List<SerializedData>));
        xmlSerializer.Serialize(stream, serializedData);

        stream.Position = 0;

        var deserializedData = (List<SerializedData>)xmlSerializer.Deserialize(stream)!;
        Assert.NotNull(deserializedData);
    }

    [Fact]
    public void Referencing()
    {
        var kernel = KernelUtils.Init();
        var serializer = kernel.GetInstance<ISerializer>();

        var referenceData = new SerializationUtils.ReferenceType { Value = 1 };
        SerializationUtils.ReferencingType.ReferenceType = referenceData;
        SerializationUtils.ReferencingType2.ReferenceType = referenceData;

        var references = new List<TupleValue<object, SerializedData>>();
        var serializedData =
            serializer.SerializeStaticFields(typeof(SerializationUtils.ReferencingType), references).ToList();
        var serializedData2 =
            serializer.SerializeStaticFields(typeof(SerializationUtils.ReferencingType2), references).ToList();
        serializedData.AddRange(serializedData2);

        Assert.Single(references);
        Assert.Equal(0, references[0].Item2.SourceReference);
        Assert.Equal(1, references[0].Item2.Fields[0].Data);
        Assert.Equal(2, serializedData.Count);
        Assert.Equal(0, serializedData[0].ReferenceData);
        Assert.Equal(0, serializedData[1].ReferenceData);

        var stream = new MemoryStream();
        var xmlSerializer = new XmlSerializer(typeof(List<SerializedData>));
        xmlSerializer.Serialize(stream, serializedData);

        stream.Position = 0;

        xmlSerializer.Deserialize(stream);
    }
}