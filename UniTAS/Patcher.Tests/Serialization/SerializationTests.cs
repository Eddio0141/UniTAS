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

        TestClassWithIntsInner(serializedData);
        Assert.Empty(serializedReferenceTypes);

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
    public void TestClassWithStrings()
    {
        var kernel = KernelUtils.Init();
        var serializer = kernel.GetInstance<ISerializer>();

        var serializedDataTuple = serializer.SerializeStaticFields(typeof(SerializationUtils.TestClassWithStrings));
        var serializedData = serializedDataTuple.Item1.ToList();
        var serializedReferenceTypes = serializedDataTuple.Item2.ToList();

        TestClassWithStringsInner(serializedData, serializedReferenceTypes);

        var streamSerializedData = new MemoryStream();
        var xmlSerializerData = new XmlSerializer(typeof(List<SerializedData>));
        xmlSerializerData.Serialize(streamSerializedData, serializedData);

        var streamSerializedReferenceTypes = new MemoryStream();
        xmlSerializerData.Serialize(streamSerializedReferenceTypes, serializedReferenceTypes);

        streamSerializedData.Position = 0;
        streamSerializedReferenceTypes.Position = 0;

        var deserializedData = (List<SerializedData>)xmlSerializerData.Deserialize(streamSerializedData)!;
        var deserializedReferenceTypes =
            (List<SerializedData>)xmlSerializerData.Deserialize(streamSerializedReferenceTypes)!;

        TestClassWithStringsInner(deserializedData, deserializedReferenceTypes);
    }

    private static void TestClassWithStringsInner(IReadOnlyList<SerializedData> serializedData,
        IReadOnlyList<SerializedData> referenceData)
    {
        Assert.Equal(3, serializedData.Count);
        Assert.Equal(typeof(SerializationUtils.TestClassWithStrings).FullName, serializedData[0].SourceClass);
        Assert.Equal(nameof(SerializationUtils.TestClassWithStrings.String1), serializedData[0].SourceField);
        Assert.Equal((uint)0, serializedData[0].ReferenceId!.Value);
        Assert.Equal(nameof(SerializationUtils.TestClassWithStrings.String2), serializedData[1].SourceField);
        Assert.Equal((uint)1, serializedData[1].ReferenceId!.Value);
        Assert.Equal(nameof(SerializationUtils.TestClassWithStrings.String3), serializedData[2].SourceField);
        Assert.True(serializedData[2].IsNullReferenceData);

        Assert.Equal(2, referenceData.Count);
        Assert.Equal((uint)0, referenceData[0].SourceReferenceId);
        Assert.Null(referenceData[0].SourceClass);
        Assert.Null(referenceData[0].SourceField);
        Assert.Equal("1", referenceData[0].Data);
        Assert.Equal((uint)1, referenceData[1].SourceReferenceId);
        Assert.Null(referenceData[1].SourceClass);
        Assert.Null(referenceData[1].SourceField);
        Assert.Equal("2", referenceData[1].Data);
    }
}