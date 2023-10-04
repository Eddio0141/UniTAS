using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace UniTAS.Patcher.Extensions;

public static class FieldInfoExtensions
{
    public static bool IsFieldUnitySerializable(this FieldInfo field)
    {
        var attrs = field.GetCustomAttributes(true);
        return (field.IsPublic && attrs.Any(x => x is NonSerializedAttribute)) || attrs.Any(x => x is SerializeField);
    }
}