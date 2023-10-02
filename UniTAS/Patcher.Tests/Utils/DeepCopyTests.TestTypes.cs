using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable CS0649
#pragma warning disable CS8618
namespace Patcher.Tests.Utils;

public partial class DeepCopyTests
{
    private class SimpleType
    {
        public int IntField;
        public string StringField;
        public SimpleType NestedType;

        public static bool ThisShouldBeIgnored;

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private const int THIS_SHOULD_BE_IGNORED_TOO = 42;
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

    private class SelfReferencing1
    {
        public SelfReferencing2 Nested;
    }

    private class SelfReferencing2
    {
        public SelfReferencing1 Nested;
    }

    private class SelfReferencing3
    {
        public SelfReferencing4 Nested;
    }

    private class SelfReferencing4
    {
        public List<SelfReferencing3> Nested;
    }
}