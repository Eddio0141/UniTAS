using System.Collections.Generic;
using UniTAS.Patcher.Utils;

namespace Patcher.Tests.Utils;

public partial class DeepCopyTests
{
    [Fact]
    public void SimpleTypeTest()
    {
        var source = new SimpleType
        {
            IntField = 42,
            StringField = "Hello World",
            NestedType = new()
            {
                IntField = 1337,
                StringField = "Hello World 2",
            },
        };
        var result = DeepCopy.MakeDeepCopy<SimpleType>(source);
        Assert.Equal(source.IntField, result.IntField);
        Assert.Equal(source.StringField, result.StringField);
        Assert.Equal(source.NestedType.IntField, result.NestedType.IntField);
        Assert.Equal(source.NestedType.StringField, result.NestedType.StringField);

        // modify reference type to check if it's a deep copy
        source.NestedType.StringField = "Hello World 3";
        Assert.NotEqual(source.NestedType.StringField, result.NestedType.StringField);

        DeepCopy.MakeDeepCopy(source, out result);

        Assert.Equal(source.IntField, result!.IntField);
        Assert.Equal(source.StringField, result.StringField);
        Assert.Equal(source.NestedType.IntField, result.NestedType.IntField);
        Assert.Equal(source.NestedType.StringField, result.NestedType.StringField);
    }

    [Fact]
    public void EnumTypeTest()
    {
        var source = new EnumType
        {
            EnumField = TestEnum.Value2,
        };
        var result = DeepCopy.MakeDeepCopy<EnumType>(source);
        Assert.Equal(source.EnumField, result.EnumField);

        DeepCopy.MakeDeepCopy(source, out result);

        Assert.Equal(source.EnumField, result!.EnumField);

        // modify to check if it's a deep copy
        source.EnumField = TestEnum.Value3;
        Assert.NotEqual(source.EnumField, result.EnumField);
    }

    [Fact]
    public void ArrayTypeTest()
    {
        var source = new ArrayType
        {
            IntArray = new[] { 1, 2, 3 },
            StringArray = new[] { "Hello", "World" },
        };
        var result = DeepCopy.MakeDeepCopy<ArrayType>(source);
        Assert.Equal(source.IntArray, result.IntArray);
        Assert.Equal(source.StringArray, result.StringArray);

        // modify reference type to check if it's a deep copy
        source.StringArray[0] = "Hello 2";
        Assert.NotEqual(source.StringArray[0], result.StringArray[0]);

        DeepCopy.MakeDeepCopy(source, out result);

        Assert.Equal(source.IntArray, result!.IntArray);
        Assert.Equal(source.StringArray, result.StringArray);
    }

    [Fact]
    public void ListTypeTest()
    {
        var source = new ListType
        {
            IntList = new() { 1, 2, 3 },
            StringList = new() { "Hello", "World" },
        };
        var result = DeepCopy.MakeDeepCopy<ListType>(source);
        Assert.Equal(source.IntList, result.IntList);
        Assert.Equal(source.StringList, result.StringList);

        // modify reference type to check if it's a deep copy
        source.StringList[1] = "Hello 2";
        source.StringList.Add("!");
        source.IntList.Add(4);
        Assert.NotEqual(source.IntList, result.IntList);
        Assert.NotEqual(source.StringList[1], result.StringList[1]);

        DeepCopy.MakeDeepCopy(source, out result);

        Assert.Equal(source.IntList, result!.IntList);
        Assert.Equal(source.StringList, result.StringList);
    }

    [Fact]
    public void EnumerableTypeTest()
    {
        var source = new EnumerableType
        {
            IntEnumerable = new List<int> { 1, 2, 3 },
            StringEnumerable = new List<string> { "Hello", "World" },
        };
        var result = DeepCopy.MakeDeepCopy<EnumerableType>(source);
        Assert.Equal(source.IntEnumerable, result.IntEnumerable);
        Assert.Equal(source.StringEnumerable, result.StringEnumerable);

        // modify reference type to check if it's a deep copy
        source.StringEnumerable = new List<string> { "Hello", "World", "!" };
        Assert.NotEqual(source.StringEnumerable, result.StringEnumerable);

        DeepCopy.MakeDeepCopy(source, out result);

        Assert.Equal(source.IntEnumerable, result!.IntEnumerable);
        Assert.Equal(source.StringEnumerable, result.StringEnumerable);
    }

    [Fact]
    public void DictionaryTypeTest()
    {
        var source = new DictionaryType
        {
            IntStringDictionary = new() { { 1, "Hello" }, { 2, "World" } },
            StringIntDictionary = new() { { "Hello", 1 }, { "World", 2 } },
        };
        var result = DeepCopy.MakeDeepCopy<DictionaryType>(source);
        Assert.Equal(source.IntStringDictionary, result.IntStringDictionary);
        Assert.Equal(source.StringIntDictionary, result.StringIntDictionary);

        // modify reference type to check if it's a deep copy
        source.IntStringDictionary[1] = "Hello 2";
        Assert.NotEqual(source.IntStringDictionary, result.IntStringDictionary);

        DeepCopy.MakeDeepCopy(source, out result);

        Assert.Equal(source.IntStringDictionary, result!.IntStringDictionary);
        Assert.Equal(source.StringIntDictionary, result.StringIntDictionary);
    }
}