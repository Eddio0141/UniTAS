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
        return PrintClass(obj, ref indent, new(new HashUtils.ReferenceComparer<object>()));
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

        // direct use cases
        if (type.IsPointer)
        {
            unsafe
            {
                var rawValue = (IntPtr)Pointer.Unbox(obj);
                return $"{initialIndent}ptr(0x{rawValue.ToInt64():X})";
            }
        }

        if (type.IsPrimitive || type.IsEnum || obj is Object and not MonoBehaviour and not ScriptableObject)
        {
            return $"{initialIndent}{obj}";
        }

        if (obj is string)
        {
            return $"{initialIndent}\"{obj}\",\n";
        }

        if (obj is Array array)
        {
            var arrayStr = $"{initialIndent}[";

            if (array.Length == 0)
            {
                return $"{arrayStr}]";
            }

            arrayStr += "\n";

            indent++;

            foreach (var item in array)
            {
                arrayStr += $"{PrintClass(item, ref indent, foundReferences)},\n";
            }

            indent--;
            return $"{initialIndent}]";
        }

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
                continue;

            var value = field.GetValue(obj);
            str += $"{IndentString(indent)}{field.Name}: {PrintClass(value, ref indent, foundReferences, true)},\n";
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