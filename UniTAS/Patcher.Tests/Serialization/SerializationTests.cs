using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using UniTAS.Patcher.Models.Serialization;
using UniTAS.Patcher.Services.Serialization;

namespace Patcher.Tests.Serialization;

public class SerializationTests
{
    [Fact]
    public void SerializeData()
    {
        var data = new SerializedData("TestClass", "TestField");
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

        var serializedDataTuple = serializer.SerializeStaticFields(typeof(SerializationUtils.TestClassWithInts));
        var serializedData = serializedDataTuple.Item1.ToList();
        var serializedReferenceTypes = serializedDataTuple.Item2;

        Assert.Equal(2, serializedData.Count);
        Assert.Equal(typeof(SerializationUtils.TestClassWithInts).FullName, serializedData[0].SourceClass);
        Assert.Equal(nameof(SerializationUtils.TestClassWithInts.Int1), serializedData[0].SourceField);
        Assert.Equal(1, serializedData[0].Data);
        Assert.Equal("Int2", serializedData[1].SourceField);
        Assert.Equal(2, serializedData[1].Data);
        Assert.Empty(serializedReferenceTypes);

        var stream = new MemoryStream();
        var xmlSerializer = new XmlSerializer(typeof(List<SerializedData>));
        xmlSerializer.Serialize(stream, serializedData);

        stream.Position = 0;

        var deserializedData = (List<SerializedData>)xmlSerializer.Deserialize(stream)!;

        Assert.Equal(2, deserializedData.Count);
        Assert.Equal(typeof(SerializationUtils.TestClassWithInts).FullName, deserializedData[0].SourceClass);
        Assert.Equal(nameof(SerializationUtils.TestClassWithInts.Int1), deserializedData[0].SourceField);
        Assert.Equal(1, deserializedData[0].Data);
        Assert.Equal("Int2", deserializedData[1].SourceField);
        Assert.Equal(2, deserializedData[1].Data);
    }
}