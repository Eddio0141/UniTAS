using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UniTAS.Patcher.Utils;

public static class DebugHelp
{
    public static string PrintClass(object obj, bool includeProperties = false)
    {
        var indent = 0;
        return PrintClass(obj, ref indent, new(), false, includeProperties);
    }

    private static string PrintClass(object obj, ref int indent, List<object> foundReferences,
        bool ignoreInitialIndent, bool includeProperties)
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

        var fields = AccessTools.GetDeclaredFields(type);

        foreach (var field in fields)
        {
            if (field.IsStatic || field.IsLiteral) continue;
            var value = field.GetValue(obj);
            str +=
                $"{IndentString(indent)}{field.Name}: {Stringify(value, foundReferences, includeProperties, ref indent)}";
        }

        if (includeProperties)
        {
            var properties = AccessTools.GetDeclaredProperties(type);

            foreach (var property in properties)
            {
                object value;
                try
                {
                    value = property.GetValue(obj, null);
                }
                catch (Exception)
                {
                    value = "(Exception thrown)";
                }

                str +=
                    $"{IndentString(indent)}{property.Name}: {Stringify(value, foundReferences, true, ref indent)}";
            }
        }

        indent--;
        str += $"{IndentString(indent)}}}";
        return str;
    }

    private static string Stringify(object value, List<object> foundReferences, bool includeProperties, ref int indent)
    {
        if (value is null)
        {
            return "null,\n";
        }

        var type = value.GetType();

        // direct use cases
        if (type.IsPointer)
        {
            unsafe
            {
                var rawValue = (IntPtr)Pointer.Unbox(value);
                return $"ptr(0x{rawValue.ToInt64():X})";
            }
        }

        if (type.IsPrimitive || type.IsEnum ||
            value is Object and not MonoBehaviour and not ScriptableObject)
        {
            return $"{value},\n";
        }

        if (value is string)
        {
            return $"\"{value}\",\n";
        }

        if (value is Array array)
        {
            if (array.Length == 0)
            {
                return "[],\n";
            }

            var str = "[\n";

            indent++;

            foreach (var item in array)
            {
                str += $"{PrintClass(item, ref indent, foundReferences, false, includeProperties)},\n";
            }

            indent--;
            str += $"{IndentString(indent)}],\n";
            return str;
        }

        // fallback
        return $"{PrintClass(value, ref indent, foundReferences, true, includeProperties)},\n";
    }

    private static string IndentString(int indent)
    {
        return new(' ', indent * 2);
    }
}