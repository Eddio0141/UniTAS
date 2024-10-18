using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Patcher.Tests.ReverseInvoker;

public partial class ReverseInvokerTests
{
    private static int Add(int a, int b)
    {
        return a + b;
    }

    private static string GenericInteract(string value)
    {
        GenericClass<string>.List.Add(value);
        return value;
    }

    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    private class GenericClass<T>
    {
        // ReSharper disable once StaticMemberInGenericType
        public static readonly List<string> List = new();

        private static void AddList(string value) => List.Add(value);
    }
}