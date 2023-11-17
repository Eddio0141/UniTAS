using System;
using System.Collections.Generic;
using System.Linq;
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

    [Fact]
    public void SelfReferencingTest()
    {
        var source = new SelfReferencing1();
        var reference2 = new SelfReferencing2();
        source.Nested = reference2;
        reference2.Nested = source;

        var result = DeepCopy.MakeDeepCopy<SelfReferencing1>(source);
        Assert.NotSame(source, result);
        Assert.NotSame(source.Nested, result.Nested);
        Assert.NotSame(source.Nested.Nested, result);

        Assert.Same(result, result.Nested.Nested);
        Assert.Same(result.Nested, result.Nested.Nested.Nested);
    }

    [Fact]
    public void SelfReferencingTest2()
    {
        var source = new SelfReferencing3();
        var reference2 = new SelfReferencing4();
        source.Nested = reference2;
        reference2.Nested = new() { source };

        var result = DeepCopy.MakeDeepCopy<SelfReferencing3>(source);
        Assert.NotSame(source, result);
        Assert.NotSame(source.Nested, result.Nested);
        Assert.NotSame(source.Nested.Nested.FirstOrDefault(), result);

        Assert.Same(result, result.Nested.Nested.FirstOrDefault());
        Assert.Same(result.Nested, result.Nested.Nested.FirstOrDefault()?.Nested);
    }

    [Fact]
    public unsafe void PointerTypeTest()
    {
        var source = new PointerType
        {
            Pointer = (void*)0x12345678,
        };
        var result = DeepCopy.MakeDeepCopy<PointerType>(source);

        Assert.Equal((IntPtr)source.Pointer, (IntPtr)result.Pointer);

        result.Pointer = (void*)0x87654321;

        Assert.NotEqual((IntPtr)source.Pointer, (IntPtr)result.Pointer);

        DeepCopy.MakeDeepCopy(source, out result);

        Assert.Equal((IntPtr)source.Pointer, (IntPtr)result!.Pointer);
    }
}