using System;
using System.Collections.Generic;
using System.Reflection;

namespace UniTAS.Patcher.Extensions;

public static class TypeExtensions
{
    public static FieldInfo[] GetFieldsRecursive(this Type type, BindingFlags bindingFlags)
    {
        if (type == null) throw new ArgumentNullException(nameof(type));

        var fields = new List<FieldInfo>();
        var currentType = type;

        while (currentType != null)
        {
            fields.AddRange(currentType.GetFields(bindingFlags));
            currentType = currentType.BaseType;
        }

        return fields.ToArray();
    }

    public static string SaneFullName(this Type type)
    {
        if (type == null) throw new ArgumentNullException(nameof(type));

        var name = type.Name;
        if (type.IsGenericParameter) return name;

        var ns = type.Namespace;
        return string.IsNullOrEmpty(ns) ? name : $"{ns}.{name}";
    }
}