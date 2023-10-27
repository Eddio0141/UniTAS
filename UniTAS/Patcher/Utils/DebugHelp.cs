using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UniTAS.Patcher.Utils;

public static class DebugHelp
{
    public static string PrintClass(object obj)
    {
        var indent = 0;
        return PrintClass(obj, ref indent, new());
    }

    private static string PrintClass(object obj, ref int indent, List<object> foundReferences,
        bool ignoreInitialIndent = false)
    {
        var type = obj.GetType();
        var str = $"{(ignoreInitialIndent ? "" : IndentString(indent))}{type.Name} {{\n";
        indent++;

        if (type.IsClass)
        {
            foundReferences.Add(obj);
        }

        var fields = AccessTools.GetDeclaredFields(obj.GetType());

        foreach (var field in fields)
        {
            if (field.IsStatic || field.IsLiteral) continue;

            str += $"{IndentString(indent)}{field.Name}: ";

            var value = field.GetValue(obj);

            // circular reference
            if (foundReferences.Contains(value))
            {
                str += "...,\n";
                continue;
            }

            var fieldType = field.FieldType;

            if (value is null)
            {
                str += "null,\n";
                continue;
            }

            // direct use cases
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
                    str += $"{PrintClass(item, ref indent, foundReferences)}\n";
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