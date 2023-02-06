using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using HarmonyLib;
using UniTASPlugin.Utils.Exceptions;

namespace UniTASPlugin.Utils;

public static class DeepCopy
{
    /// <summary>
    /// A cache for the <see cref="ICollection{T}.Add"/> or similar Add methods for different types.
    /// </summary>
    private static readonly Dictionary<Type, FastInvokeHandler> AddHandlerCache = new();

    private static readonly ReaderWriterLock AddHandlerCacheLock = new();

    /// <summary>Makes a deep copy of any object</summary>
    /// <typeparam name="T">The type of the instance that should be created; for legacy reasons, this must be a class or interface</typeparam>
    /// <param name="source">The original object</param>
    /// <returns>A copy of the original object but of type T</returns>
    ///
    public static T MakeDeepCopy<T>(object source) where T : class
    {
        return MakeDeepCopy(source, typeof(T)) as T;
    }

    /// <summary>Makes a deep copy of any object</summary>
    /// <typeparam name="T">The type of the instance that should be created</typeparam>
    /// <param name="source">The original object</param>
    /// <param name="result">[out] The copy of the original object</param>
    /// <param name="processor">Optional value transformation function (taking a field name and src/dst <see cref="Traverse"/> instances)</param>
    /// <param name="pathRoot">The optional path root to start with</param>
    ///
    public static void MakeDeepCopy<T>(object source, out T result,
        Func<string, Traverse, Traverse, object> processor = null, string pathRoot = "")
    {
        result = (T)MakeDeepCopy(source, typeof(T), processor, pathRoot);
    }

    private static int _makeDeepCopyRecursionDepth;

    private const int MakeDeepCopyRecursionDepthLimit = 200;

    /// <summary>Makes a deep copy of any object</summary>
    /// <param name="source">The original object</param>
    /// <param name="resultType">The type of the instance that should be created</param>
    /// <param name="processor">Optional value transformation function (taking a field name and src/dst <see cref="Traverse"/> instances)</param>
    /// <param name="pathRoot">The optional path root to start with</param>
    /// <returns>The copy of the original object</returns>
    public static object MakeDeepCopy(object source, Type resultType,
        Func<string, Traverse, Traverse, object> processor = null, string pathRoot = "")
    {
        _makeDeepCopyRecursionDepth++;
        if (_makeDeepCopyRecursionDepth > MakeDeepCopyRecursionDepthLimit)
        {
            _makeDeepCopyRecursionDepth = 0;
            throw new DeepCopyMaxRecursionException();
        }

        if (source is null || resultType is null)
        {
            _makeDeepCopyRecursionDepth--;
            return null;
        }

        resultType = Nullable.GetUnderlyingType(resultType) ?? resultType;
        var type = source.GetType();

        if (type.IsPrimitive)
        {
            _makeDeepCopyRecursionDepth--;
            return source;
        }

        if (type.IsEnum)
        {
            _makeDeepCopyRecursionDepth--;
            return Enum.ToObject(resultType, (int)source);
        }

        if (type.IsGenericType && resultType.IsGenericType)
        {
            AddHandlerCacheLock.AcquireReaderLock(200);
            try
            {
                if (!AddHandlerCache.TryGetValue(resultType, out var addInvoker))
                {
                    var addOperation = AccessTools.FirstMethod(resultType,
                        m => m.Name == "Add" && m.GetParameters().Length == 1);
                    if (addOperation is not null)
                    {
                        addInvoker = MethodInvoker.GetHandler(addOperation);
                    }

                    _ = AddHandlerCacheLock.UpgradeToWriterLock(200);
                    AddHandlerCacheLock.AcquireWriterLock(200);
                    try
                    {
                        AddHandlerCache[resultType] = addInvoker;
                    }
                    finally
                    {
                        AddHandlerCacheLock.ReleaseWriterLock();
                    }
                }

                if (addInvoker != null)
                {
                    var addableResult = Activator.CreateInstance(resultType);
                    var newElementType = resultType.GetGenericArguments()[0];
                    var i = 0;
                    foreach (var element in (IEnumerable)source)
                    {
                        var iStr = i++.ToString();
                        var path = pathRoot.Length > 0 ? pathRoot + "." + iStr : iStr;
                        var newElement = MakeDeepCopy(element, newElementType, processor, path);
                        _ = addInvoker(addableResult, newElement);
                    }

                    _makeDeepCopyRecursionDepth--;
                    return addableResult;
                }
            }
            finally
            {
                AddHandlerCacheLock.ReleaseReaderLock();
            }
        }

        if (type.IsArray && resultType.IsArray)
        {
            var newElementType = resultType.GetElementType();
            var array = (Array)source;
            var newArray = Array.CreateInstance(newElementType ?? throw new InvalidOperationException(), array.Length);
            for (var i = 0; i < array.Length; i++)
            {
                var iStr = i.ToString();
                var path = pathRoot.Length > 0 ? pathRoot + "." + iStr : iStr;
                var newElement = MakeDeepCopy(array.GetValue(i), newElementType, processor, path);
                newArray.SetValue(newElement, i);
            }

            _makeDeepCopyRecursionDepth--;
            return newArray;
        }

        // is type a collection?
        if (typeof(IEnumerable).IsAssignableFrom(type) && typeof(IEnumerable).IsAssignableFrom(resultType))
        {
            var sourceCollection = (IEnumerable)source;

            var resultTypeInterface = resultType.GetInterfaces()
                .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
            var resultTypeGenericArgument = resultTypeInterface.GetGenericArguments()[0];
            var iEnumerableType = typeof(IEnumerable<>).MakeGenericType(resultTypeGenericArgument);

            var tempResultList =
                (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(resultTypeGenericArgument));
            foreach (var element in sourceCollection)
            {
                var newElement = MakeDeepCopy(element, resultTypeGenericArgument, processor, pathRoot);
                tempResultList.Add(newElement);
            }

            var addableResult = AccessTools.Constructor(resultType, new[] { iEnumerableType })
                .Invoke(new object[] { tempResultList });

            _makeDeepCopyRecursionDepth--;
            return addableResult;
        }

        var ns = type.Namespace;
        if (ns == "System" || (ns?.StartsWith("System.") ?? false))
        {
            _makeDeepCopyRecursionDepth--;
            return source;
        }

        var result = AccessTools.CreateInstance(resultType == typeof(object) ? type : resultType);
        Traverse.IterateFields(source, result, (name, src, dst) =>
        {
            // stupid hack to get FieldInfo from Traverse
            var srcField = Traverse.Create(src).Field("_info").GetValue<FieldInfo>();
            if (srcField is null)
            {
                throw new NullReferenceException("srcField is null, this should never happen");
            }

            if (srcField.IsStatic || srcField.IsLiteral)
            {
                return;
            }

            var path = pathRoot.Length > 0 ? pathRoot + "." + name : name;
            var value = processor is not null ? processor(path, src, dst) : src.GetValue();
            dst.SetValue(MakeDeepCopy(value, dst.GetValueType(), processor, path));
        });
        _makeDeepCopyRecursionDepth--;
        return result;
    }
}