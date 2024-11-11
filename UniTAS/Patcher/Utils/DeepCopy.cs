using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UniTAS.Patcher.Exceptions;
using UniTAS.Patcher.Extensions;
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
    /// <param name="pathRoot">The optional path root to start with</param>
    ///
    public static void MakeDeepCopy<T>(object source, out T result,
        Processor processor = null, string pathRoot = "")
    {
        result = (T)MakeDeepCopy(source, processor, pathRoot);
    }

    private static int _makeDeepCopyRecursionDepth;

    private const int MakeDeepCopyRecursionDepthLimit = 200;

    /// <summary>Makes a deep copy of any object</summary>
    /// <param name="source">The original object</param>
    /// <param name="processor">Optional custom copy function (taking a field name, src <see cref="Traverse"/>, and an output copied object. Return is to use this object or not)</param>
    /// <param name="pathRoot">The optional path root to start with</param>
    /// <returns>The copy of the original object</returns>
    public static object MakeDeepCopy(object source, Processor processor = null,
        string pathRoot = "")
    {
        return MakeDeepCopy(source, processor, pathRoot, new(new HashUtils.ReferenceComparer()));
    }

    // references: (before, after)
    private static object MakeDeepCopy(object source, Processor processor, string pathRoot,
        Dictionary<object, object> references)
    {
        StaticLogger.Trace(
            $"MakeDeepCopy, depth: {_makeDeepCopyRecursionDepth}, type: {source?.GetType().FullName}, pathRoot: {pathRoot}");

        if (processor is not null)
        {
            if (source != null)
            {
                if (references.TryGetValue(source, out var reference))
                {
                    return reference;
                }
            }

            if (processor(pathRoot, source, out var processorValue))
            {
                StaticLogger.Trace("MakeDeepCopy, using processor to get copied value");
                return processorValue;
            }
        }

        if (source is null)
        {
            StaticLogger.Trace("MakeDeepCopy, returning null");
            return null;
        }

        if (source is Object and not ScriptableObject)
        {
            // this is a native unity object, so we can't make a deep copy of it
            StaticLogger.Trace("MakeDeepCopy, skipping native unity object");
            return source;
        }

        if (references.TryGetValue(source, out var referenceEntry))
        {
            StaticLogger.Trace($"MakeDeepCopy, returning existing reference");
            return referenceEntry;
        }

        if (source is IntPtr or UIntPtr)
        {
            StaticLogger.LogDebug($"DeepCopy: received pointer type: `{source}`, may be unmanaged");
        }

        var type = source.GetType();

        if (type.IsPrimitive || type == typeof(string))
        {
            StaticLogger.Trace("MakeDeepCopy, returning primitive");
            return source;
        }

        if (type.IsEnum)
        {
            return source;
        }

        _makeDeepCopyRecursionDepth++;

        if (_makeDeepCopyRecursionDepth > MakeDeepCopyRecursionDepthLimit)
        {
            throw new DeepCopyMaxRecursionException(source, pathRoot);
        }

        if (type.IsArray)
        {
            StaticLogger.Trace("MakeDeepCopy, copying array");
            var newElementType = type.GetElementType();
            var array = (Array)source;
            var newArray = Array.CreateInstance(newElementType ?? throw new InvalidOperationException(), array.Length);
            references.Add(source, newArray);
            for (var i = 0; i < array.Length; i++)
            {
                var iStr = i.ToString();
                var path = pathRoot.Length > 0 ? pathRoot + "." + iStr : iStr;
                var newElement = MakeDeepCopy(array.GetValue(i), processor, path, references);
                newArray.SetValue(newElement, i);
            }

            _makeDeepCopyRecursionDepth--;
            StaticLogger.Trace("MakeDeepCopy, returning array");
            return newArray;
        }

        // is type a collection?
        // if (typeof(IEnumerable).IsAssignableFrom(type))
        // {
        //     StaticLogger.Trace("MakeDeepCopy, copying IEnumerable");
        //     var sourceCollection = (IEnumerable)source;
        //     foundReferences.Add(id, source);
        //     var newRefId = id;
        //     id++;
        //
        //     Type resultTypeInterface;
        //     if (type.IsInterface)
        //     {
        //         resultTypeInterface = type;
        //     }
        //     else
        //     {
        //         resultTypeInterface = type.GetInterfaces().First(i =>
        //             i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        //     }
        //
        //     var resultTypeGenericArgument = resultTypeInterface.GetGenericArguments()[0];
        //     var iEnumerableType = typeof(IEnumerable<>).MakeGenericType(resultTypeGenericArgument);
        //
        //     var ctor = AccessTools.Constructor(type, new[] { iEnumerableType });
        //     if (ctor != null)
        //     {
        //         var tempResultList =
        //             (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(resultTypeGenericArgument));
        //         foreach (var element in sourceCollection)
        //         {
        //             var newElement = MakeDeepCopy(element, processor, pathRoot,
        //                 foundReferences, newReferences, ref id, ref a);
        //             tempResultList.Add(newElement);
        //         }
        //
        //         var addableResult = ctor.Invoke(new object[] { tempResultList });
        //
        //         _makeDeepCopyRecursionDepth--;
        //         newReferences.Add(newRefId, addableResult);
        //         StaticLogger.Trace("MakeDeepCopy, returning IEnumerable");
        //         if (a > 0) a++;
        //         return addableResult;
        //     }
        //
        //     StaticLogger.Trace("MakeDeepCopy, IEnumerable has no constructor with IEnumerable argument");
        // }

        // var ns = type.Namespace;
        // if (ns == "System" || (ns?.StartsWith("System.") ?? false))
        // {
        //     _makeDeepCopyRecursionDepth--;
        //     StaticLogger.Trace($"MakeDeepCopy, returning system type, namespace: {ns}");
        //     if (a > 0) a++;
        //     return source;
        // }

        StaticLogger.Trace("MakeDeepCopy, creating new instance and copying fields");

        var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic |
                                    BindingFlags.GetField | BindingFlags.SetField).Where(f => !f.IsLiteral);

        object result;

        var typeFullName = type.SaneFullName();

        if (source is AnimationCurve || typeFullName == "UnityEngine.AnimationCurve.AnimationCurve")
        {
            // call the default ctor
            result = Activator.CreateInstance(type);
            // also ignore unmanaged stuff
            fields = fields.Where(f => !(f.Name == "m_Ptr" && f.FieldType == typeof(IntPtr)));
        }
        else
        {
            result = source is ScriptableObject
                ? ScriptableObject.CreateInstance(type)
                : AccessTools.CreateInstance(type);
        }

        // guaranteed to be a reference type
        references.Add(source, result);

        foreach (var field in fields)
        {
            var name = field.Name;
            StaticLogger.Trace($"MakeDeepCopy, processing field: {name}");
            var path = pathRoot.Length > 0 ? pathRoot + "." + name : name;

            var value = field.GetValue(source);

            if (field.FieldType.IsPointer)
            {
                StaticLogger.Trace("MakeDeepCopy, copying pointer field");
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

            StaticLogger.Trace("MakeDeepCopy, copying field");
            var copiedObj = MakeDeepCopy(value, processor, path, references);

            field.SetValue(result, copiedObj);
        }

        _makeDeepCopyRecursionDepth--;
        StaticLogger.Trace("MakeDeepCopy, returning result from copied fields");
        return result;
    }

    public delegate bool Processor(string path, object source, out object copiedObj);
}