using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Patcher.Tests.ReverseInvoker;

public partial class ReverseInvokerTests
{
    private static int Add(int a, int b)
    {
        return a + b;
    }

    private static T Generic<T>(T value) => value;

    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    private class GenericClass<T>
    {
        public static T Value = default!;
        public static readonly List<string> List = new();

        private static T InnerGeneric(T value) => Generic(value);

        private static void SetValue(T value) => Value = value;

        private static void AddList(string value) => List.Add(value);
    }
}