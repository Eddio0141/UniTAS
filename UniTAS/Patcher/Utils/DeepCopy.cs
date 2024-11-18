using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UniTAS.Patcher.Exceptions;
using UniTAS.Patcher.Extensions;
using UniTAS.Patcher.ManualServices;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UniTAS.Patcher.Utils;

public static class DeepCopy
{
    /// <summary>Makes a deep copy of any object</summary>
    /// <typeparam name="T">The type of the instance that should be created; for legacy reasons, this must be a class or interface</typeparam>
    /// <param name="source">The original object</param>
    /// <returns>A copy of the original object but of type T</returns>
    public static T MakeDeepCopy<T>(object source) where T : class
    {
        return MakeDeepCopy(source) as T;
    }

    /// <summary>Makes a deep copy of any object</summary>
    /// <typeparam name="T">The type of the instance that should be created</typeparam>
    /// <param name="source">The original object</param>
    /// <param name="result">[out] The copy of the original object</param>
    /// <param name="processor">Optional custom copy function (taking a field name, src <see cref="Traverse"/>, and an output copied object. Return is to use this object or not)</param>
    public static void MakeDeepCopy<T>(object source, out T result, Processor processor = null)
    {
        result = (T)MakeDeepCopy(source, processor);
    }

    private static int _makeDeepCopyRecursionDepth;

    private const int MakeDeepCopyRecursionDepthLimit = 200;

    /// <summary>Makes a deep copy of any object</summary>
    /// <param name="source">The original object</param>
    /// <param name="processor">Optional custom copy function (taking a field name, src <see cref="Traverse"/>, and an output copied object. Return is to use this object or not)</param>
    /// <returns>The copy of the original object</returns>
    public static object MakeDeepCopy(object source, Processor processor = null)
    {
        return MakeDeepCopy(source, processor, new(new HashUtils.ReferenceComparer<object>()));
    }

    // references: (before, after)
    private static object MakeDeepCopy(object source, Processor processor, Dictionary<object, object> references)
    {
        using var _ = Bench.Measure();

        if (processor is not null)
        {
            if (source != null)
            {
                if (references.TryGetValue(source, out var reference))
                {
                    return reference;
                }
            }

            if (processor(source, out var processorValue))
            {
                return processorValue;
            }
        }

        if (source is null)
        {
            return null;
        }

        if (source is Object and not ScriptableObject)
        {
            // this is a native unity object, so we can't make a deep copy of it
            return source;
        }

        if (references.TryGetValue(source, out var referenceEntry))
        {
            return referenceEntry;
        }

        if (source is IntPtr or UIntPtr)
        {
            StaticLogger.LogDebug($"DeepCopy: received pointer type: `{source}`, may be unmanaged");
        }

        var type = source.GetType();

        if (type.IsPrimitive || type == typeof(string))
        {
            return source;
        }

        if (type.IsEnum)
        {
            return source;
        }

        _makeDeepCopyRecursionDepth++;

        if (_makeDeepCopyRecursionDepth > MakeDeepCopyRecursionDepthLimit)
        {
            throw new DeepCopyMaxRecursionException(source);
        }

        if (type.IsArray)
        {
            var newElementType = type.GetElementType();
            var array = (Array)source;
            var newArray = Array.CreateInstance(newElementType ?? throw new InvalidOperationException(), array.Length);
            references.Add(source, newArray);
            for (var i = 0; i < array.Length; i++)
            {
                var newElement = MakeDeepCopy(array.GetValue(i), processor, references);
                newArray.SetValue(newElement, i);
            }

            _makeDeepCopyRecursionDepth--;
            return newArray;
        }

        FieldInfo[] fields;

        object result;

        var typeFullName = type.SaneFullName();

        if (source is AnimationCurve || typeFullName == "UnityEngine.AnimationCurve.AnimationCurve")
        {
            fields = FieldInfoCache.GetOrAdd(type, t =>
            {
                return t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic |
                                   BindingFlags.GetField | BindingFlags.SetField).Where(f => !f.IsLiteral)
                    .Where(f => !(f.Name == "m_Ptr" && f.FieldType == typeof(IntPtr))).ToArray();
            });

            // call the default ctor
            result = Activator.CreateInstance(type);
        }
        else
        {
            fields = FieldInfoCache.GetOrAdd(type, t => t.GetFields(BindingFlags.Instance | BindingFlags.Public |
                                                                    BindingFlags.NonPublic |
                                                                    BindingFlags.GetField | BindingFlags.SetField)
                .Where(f => !f.IsLiteral).ToArray());

            result = source is ScriptableObject
                ? ScriptableObject.CreateInstance(type)
                : AccessTools.CreateInstance(type);
        }

        // guaranteed to be a reference type
        references.Add(source, result);

        foreach (var field in fields)
        {
            var value = field.GetValue(source);

            if (field.FieldType.IsPointer)
            {
                unsafe
                {
                    field.SetValue(result, (IntPtr)Pointer.Unbox(value));
                }

                continue;
            }

            if (field.FieldType == typeof(IntPtr) || field.FieldType == typeof(UIntPtr))
            {
                StaticLogger.LogDebug($"DeepCopy: found pointer field `{typeFullName}.{field.Name}`, may be unmanaged");
            }

            var copiedObj = MakeDeepCopy(value, processor, references);

            field.SetValue(result, copiedObj);
        }

        _makeDeepCopyRecursionDepth--;
        return result;
    }

    private static readonly ConcurrentDictionary<Type, FieldInfo[]> FieldInfoCache = [];

    // path removed because unused right now
    // public delegate bool Processor(string path, object source, out object copiedObj);
    public delegate bool Processor(object source, out object copiedObj);
}