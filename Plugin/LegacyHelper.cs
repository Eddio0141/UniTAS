using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using HarmonyLib;
using UniTASPlugin.LegacyExceptions;
using UniTASPlugin.LegacySafeWrappers;
using UniTASPlugin.ReverseInvoker;
using UnityEngine;

namespace UniTASPlugin;

public static class Helper
{
    public static string GetUnityVersion()
    {
        const string unityPlayerPath = @".\UnityPlayer.dll";
        string versionRaw;
        var rev = Plugin.Kernel.GetInstance<PatchReverseInvoker>();
        if (rev.Invoke(System.IO.File.Exists, unityPlayerPath))
        {
            var fullPath = rev.Invoke(System.IO.Path.GetFullPath, unityPlayerPath);
            var fileVersion = FileVersionInfo.GetVersionInfo(fullPath);
            versionRaw = fileVersion.FileVersion;
        }
        else
        {
            versionRaw = Application.unityVersion;
        }

        return versionRaw;
    }

    public static bool ValueHasDecimalPoints(float value)
    {
        return value.ToString().Contains(".");
    }

    public static Assembly[] GetGameAssemblies()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var resetIgnoreAssemblies = new[]
        {
            "mscorlib",
            "BepInEx.Preloader",
            "BepInEx",
            "System.Core",
            "0Harmony",
            "System",
            "HarmonyXInterop",
            "System.Configuration",
            "System.Xml",
            "DemystifyExceptions",
            "StartupProfiler",
            "Purchasing.Common",
            "netstandard",
            "UniTASPlugin"
        };
        var resetIgnoreAssmelibes_startsWith = new[]
        {
            "Unity.",
            "UnityEngine.",
            "Mono.",
            "MonoMod.",
            "HarmonyDTFAssembly"
        };

        return assemblies.Where(assembly =>
        {
            foreach (var assemblyCheck in resetIgnoreAssmelibes_startsWith)
                if (assembly.FullName.StartsWith(assemblyCheck))
                    return false;
            foreach (var assemblyCheck in resetIgnoreAssemblies)
                if (assembly.FullName == assemblyCheck)
                    return false;
            return true;
        }).ToArray();
    }

    public static string GameRootDir()
    {
        var appBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
        var rev = Plugin.Kernel.GetInstance<PatchReverseInvoker>();
        return appBase ?? rev.Invoke(System.IO.Path.GetFullPath, ".");
    }

    public static string GameName()
    {
        return AppInfo.ProductName();
    }

    public static string GameExePath()
    {
        // TODO other platform support that's not windows
        var rev = Plugin.Kernel.GetInstance<PatchReverseInvoker>();
        return rev.Invoke(System.IO.Path.Combine, GameRootDir(), $"{GameName()}.exe");
    }

    /// <summary>
    /// A cache for the <see cref="ICollection{T}.Add"/> or similar Add methods for different types.
    /// </summary>
    private static readonly Dictionary<Type, FastInvokeHandler> addHandlerCache = new();

    private static readonly ReaderWriterLock addHandlerCacheLock = new();

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

    private static int MakeDeepCopyRecursionDepth;

    private const int MakeDeepCopyRecursionDepthLimit = 500;

    /// <summary>Makes a deep copy of any object</summary>
    /// <param name="source">The original object</param>
    /// <param name="resultType">The type of the instance that should be created</param>
    /// <param name="processor">Optional value transformation function (taking a field name and src/dst <see cref="Traverse"/> instances)</param>
    /// <param name="pathRoot">The optional path root to start with</param>
    /// <returns>The copy of the original object</returns>
    public static object MakeDeepCopy(object source, Type resultType,
        Func<string, Traverse, Traverse, object> processor = null, string pathRoot = "")
    {
        MakeDeepCopyRecursionDepth++;
        if (MakeDeepCopyRecursionDepth > MakeDeepCopyRecursionDepthLimit)
        {
            MakeDeepCopyRecursionDepth = 0;
            throw new DeepCopyMaxRecursion();
        }

        if (source is null || resultType is null)
        {
            MakeDeepCopyRecursionDepth--;
            return null;
        }

        resultType = Nullable.GetUnderlyingType(resultType) ?? resultType;
        var type = source.GetType();

        if (type.IsPrimitive)
        {
            MakeDeepCopyRecursionDepth--;
            return source;
        }

        if (type.IsEnum)
        {
            MakeDeepCopyRecursionDepth--;
            return Enum.ToObject(resultType, (int)source);
        }

        if (type.IsGenericType && resultType.IsGenericType)
        {
            addHandlerCacheLock.AcquireReaderLock(200);
            try
            {
                if (!addHandlerCache.TryGetValue(resultType, out var addInvoker))
                {
                    var addOperation = AccessTools.FirstMethod(resultType,
                        m => m.Name == "Add" && m.GetParameters().Length == 1);
                    if (addOperation is not null)
                    {
                        addInvoker = MethodInvoker.GetHandler(addOperation);
                    }

                    _ = addHandlerCacheLock.UpgradeToWriterLock(200);
                    addHandlerCacheLock.AcquireWriterLock(200);
                    try
                    {
                        addHandlerCache[resultType] = addInvoker;
                    }
                    finally
                    {
                        addHandlerCacheLock.ReleaseWriterLock();
                    }
                }

                if (addInvoker != null)
                {
                    var addableResult = Activator.CreateInstance(resultType);
                    var newElementType = resultType.GetGenericArguments()[0];
                    var i = 0;
                    foreach (var element in source as IEnumerable)
                    {
                        var iStr = i++.ToString();
                        var path = pathRoot.Length > 0 ? pathRoot + "." + iStr : iStr;
                        var newElement = MakeDeepCopy(element, newElementType, processor, path);
                        _ = addInvoker(addableResult, newElement);
                    }

                    MakeDeepCopyRecursionDepth--;
                    return addableResult;
                }
            }
            finally
            {
                addHandlerCacheLock.ReleaseReaderLock();
            }
        }

        if (type.IsArray && resultType.IsArray)
        {
            var newElementType = resultType.GetElementType();
            var array = source as Array;
            var newArray = Array.CreateInstance(newElementType, array.Length);
            for (var i = 0; i < array.Length; i++)
            {
                var iStr = i.ToString();
                var path = pathRoot.Length > 0 ? pathRoot + "." + iStr : iStr;
                var newElement = MakeDeepCopy(array.GetValue(i), newElementType, processor, path);
                newArray.SetValue(newElement, i);
            }

            MakeDeepCopyRecursionDepth--;
            return newArray;
        }

        // is type a collection?
        if (typeof(ICollection).IsAssignableFrom(type) && typeof(ICollection).IsAssignableFrom(resultType))
        {
            var addableResult = Activator.CreateInstance(resultType);
            var addOperation = AccessTools.FirstMethod(resultType,
                m => m.Name == "Add" && m.GetParameters().Length == 1);
            if (addOperation is null)
            {
                throw new($"Cannot add to collection, source type: {type}, result type: {resultType}");
            }

            var addInvoker = MethodInvoker.GetHandler(addOperation);
            var elementType = addOperation.GetParameters()[0].ParameterType;
            var i = 0;
            foreach (var element in source as IEnumerable)
            {
                var iStr = i++.ToString();
                var path = pathRoot.Length > 0 ? pathRoot + "." + iStr : iStr;
                var newElement = MakeDeepCopy(element, elementType, processor, path);
                addInvoker(addableResult, newElement);
            }

            MakeDeepCopyRecursionDepth--;
            return addableResult;
        }

        var ns = type.Namespace;
        if (ns == "System" || (ns?.StartsWith("System.") ?? false))
        {
            MakeDeepCopyRecursionDepth--;
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
        MakeDeepCopyRecursionDepth--;
        return result;
    }

    public static string WildCardToRegular(string value)
    {
        return "^" + Regex.Escape(value).Replace("\\?", ".").Replace("\\*", ".*") + "$";
    }
}