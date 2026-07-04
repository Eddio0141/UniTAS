using System;
using System.Reflection;
using UnityEngine;

namespace UniTAS.Patcher.Extensions;

public static class FieldInfoExtensions
{
    public static bool IsFieldUnitySerializable(this FieldInfo field)
    {
        return field.GetCustomAttributes(typeof(NonSerializedAttribute), true).Length == 0 && (field.IsPublic || field.GetCustomAttributes(typeof(SerializeField), true).Length > 0);
    }
}
