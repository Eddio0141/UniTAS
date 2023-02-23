﻿#pragma warning disable CS0649
#pragma warning disable CS8618
namespace UniTAS.Plugin.Tests.Utils;

public partial class DeepCopyTests
{
    private class SimpleType
    {
        public int IntField;
        public string StringField;
        public SimpleType NestedType;
    }

    private enum TestEnum
    {
        Value2,
        Value3,
    }

    private class EnumType
    {
        public TestEnum EnumField;
    }

    private class ArrayType
    {
        public int[] IntArray;
        public string[] StringArray;
    }
    
    private class ListType
    {
        public List<int> IntList;
        public List<string> StringList;
    }
    
    private class EnumerableType
    {
        public IEnumerable<int> IntEnumerable;
        public IEnumerable<string> StringEnumerable;
    }
    
    private class DictionaryType
    {
        public Dictionary<int, string> IntStringDictionary;
        public Dictionary<string, int> StringIntDictionary;
    }
}