using System;

namespace UniTAS.Patcher.Extensions;

public static class TypeExtensions
{
    public static string SaneFullName(this Type type)
    {
        if (type == null) throw new ArgumentNullException(nameof(type));

        var name = type.Name;
        if (type.IsGenericParameter) return name;

        var ns = type.Namespace;
        return string.IsNullOrEmpty(ns) ? name : $"{ns}.{name}";
    }
}