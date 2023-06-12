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

        var serializedData = serializer.SerializeStaticFields(typeof(SerializationUtils.TestClassWithInts)).Item1
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

        var serializedData = serializer.SerializeStaticFields(typeof(SerializationUtils.InstanceLoop)).Item1.ToList();
        var stream = new MemoryStream();
        var xmlSerializer = new XmlSerializer(typeof(List<SerializedData>));
        xmlSerializer.Serialize(stream, serializedData);

        stream.Position = 0;

        var deserializedData = (List<SerializedData>)xmlSerializer.Deserialize(stream)!;
        Assert.NotNull(deserializedData);
    }
}