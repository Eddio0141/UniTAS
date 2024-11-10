using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UniTAS.Patcher.Utils;

public static class DebugHelp
{
    [UsedImplicitly]
    public static string PrintClass(object obj)
    {
        var indent = 0;
        return PrintClass(obj, ref indent, []);
    }

    private static string PrintClass(object obj, ref int indent, HashSet<object> foundReferences,
        bool ignoreInitialIndent = false)
    {
        var initialIndent = $"{(ignoreInitialIndent ? "" : IndentString(indent))}";

        if (obj == null)
        {
            return $"{initialIndent}null";
        }

        // circular reference
        if (foundReferences.Contains(obj))
        {
            return $"{initialIndent}...";
        }

        var type = obj.GetType();
        var str = $"{initialIndent}{type.Name} {{\n";
        indent++;

        if (type.IsClass && type != typeof(string))
        {
            foundReferences.Add(obj);
        }

        var fields = type.GetFields(AccessTools.all);

        foreach (var field in fields)
        {
            if (field.IsStatic || field.IsLiteral)
            {
                continue;
            }

            str += $"{IndentString(indent)}{field.Name}: ";

            var value = field.GetValue(obj);

            if (value is null)
            {
                str += "null,\n";
                continue;
            }

            var fieldType = field.FieldType;

            // direct use cases
            if (fieldType.IsPointer)
            {
                unsafe
                {
                    var rawValue = (IntPtr)Pointer.Unbox(value);
                    str += $"ptr(0x{rawValue.ToInt64():X})";
                }

                continue;
            }

            if (fieldType.IsPrimitive || fieldType.IsEnum ||
                value is Object and not MonoBehaviour and not ScriptableObject)
            {
                str += $"{value},\n";
                continue;
            }

            if (value is string)
            {
                str += $"\"{value}\",\n";
                continue;
            }

            if (value is Array array)
            {
                str += "[";

                if (array.Length == 0)
                {
                    str += "],\n";
                    continue;
                }

                str += "\n";

                indent++;

                foreach (var item in array)
                {
                    str += $"{PrintClass(item, ref indent, foundReferences)},\n";
                }

                indent--;
                str += $"{IndentString(indent)}],\n";
                continue;
            }

            // fallback
            str += $"{PrintClass(value, ref indent, foundReferences, true)},\n";
        }

        indent--;
        str += $"{IndentString(indent)}}}";
        return str;
    }

    private static string IndentString(int indent)
    {
        return new(' ', indent * 2);
    }
}