using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Mono.Cecil;
using MonoMod.Utils;
using UniTAS.Patcher.Runtime;
using UniTAS.Plugin.Extensions;
using UniTAS.Plugin.Logger;

namespace UniTAS.Plugin.StaticFieldStorage;

// ReSharper disable once ClassNeverInstantiated.Global
public class StaticFieldStorage : IStaticFieldManipulator
{
    private readonly ILogger _logger;

    // list of static fields to set to null
    private readonly List<FieldInfo> _staticFields = new();

    public StaticFieldStorage(ILogger logger)
    {
        _logger = logger;

        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var genericType = new List<Type>();

        // TODO sort out situation where nuget package needs to be added automatically
        // ReSharper disable StringLiteralTypo
        var assemblyExclusionsRaw = new[]
        {
            "UnityEngine.*",
            "UnityEngine",
            "Unity.*",
            "System.*",
            "System",
            "netstandard",
            "mscorlib",
            "Mono.*",
            "Mono",
            "MonoMod.*",
            "BepInEx.*",
            "BepInEx",
            "MonoMod.*",
            "0Harmony",
            "HarmonyXInterop",
            "StructureMap"
        };
        // ReSharper restore StringLiteralTypo

        var assemblyExclusions = new[]
        {
            typeof(StaticFieldStorage).Assembly,
            typeof(UniTAS.Patcher.Patcher).Assembly,
            typeof(MoonSharp.Interpreter.Script).Assembly,
            typeof(ImmutableList<>).Assembly
        };

        foreach (var assembly in assemblies)
        {
            if (assemblyExclusionsRaw.Any(x => assembly.GetName().Name.Like(x))) continue;
            if (assemblyExclusions.Any(x => Equals(x, assembly))) continue;

            Trace.Write($"Processing assembly {assembly.GetName().Name} for static fields and constructor");

            var types = assembly.GetTypes();

            foreach (var type in types)
            {
                // if type has generic parameters
                if (type.IsGenericType)
                {
                    genericType.Add(type);
                    continue;
                }

                var fields = AccessTools.GetDeclaredFields(type).Where(x =>
                    x.IsStatic && !x.IsLiteral).ToArray();

                if (fields.Length == 0 && type.TypeInitializer == null) continue;

#if TRACE
                Trace.Write(
                    $"storing static fields for type: {type.FullName}, field count: {fields.Length}, and static constructor: {type.TypeInitializer != null}");
                foreach (var field in fields)
                {
                    Trace.Write($"found field {type.FullName}.{field.Name}");
                }
#endif
                _staticFields.AddRange(fields);
            }
        }

        if (genericType.Count == 0) return;

// find all types for each generic type
        var assemblyDefinitions = new List<AssemblyDefinition>();
        foreach (var assembly in assemblies)
        {
            try
            {
                assemblyDefinitions.Add(AssemblyDefinition.ReadAssembly(assembly.Location));
            }
            catch (Exception e)
            {
                // ignore
                _logger.LogWarning($"Failed to read assembly {assembly.FullName} with error: {e.Message}");
            }
        }

// Link of generic types, and list of generic args (so a list of lists)
        var genericTypeUsedGenericTypes = new Dictionary<Type, List<List<Type>>>();

// this is cursed but it works
// iterates through all il instructions in all methods in all types in all assemblies to find usage of generic types
        foreach (var instruction in from assemblyDefinition in assemblyDefinitions
                 from moduleDefinition in assemblyDefinition.Modules
                 from typeDefinition in moduleDefinition.Types
                 from methodDefinition in typeDefinition.Methods
                 where methodDefinition.HasBody
                 from instruction in methodDefinition.Body.Instructions
                 select instruction)
        {
            if (instruction.Operand is not MemberReference
                {
                    DeclaringType: GenericInstanceType genericInstanceType
                }) continue;

            // if the generic type is not in the list of generic types we are looking for, skip
            var actualType = genericInstanceType.ElementType.ResolveReflection();
            var genericTypeFound = genericType.FirstOrDefault(x => x == actualType);

            if (genericTypeFound == null) continue;

            // skip if generic type is generic itself
            // recursively check
            if (FindGenericType(genericInstanceType))
            {
                Trace.Write($"Skipping generic type {genericInstanceType.FullName} for static field storage");
                continue;
            }

            Trace.Write($"Resolving generic type {genericInstanceType.FullName} for static field storage");
            var allGenericTypes = genericInstanceType.GenericArguments
                .Select(x => x.ResolveReflection()).ToList();

            // add if allGenericTypes is not already in the list
            if (!genericTypeUsedGenericTypes.TryGetValue(genericTypeFound, out var list))
            {
                genericTypeUsedGenericTypes.Add(genericTypeFound, new() { allGenericTypes });
            }
            else
            {
                if (list.Any(x => x.SequenceEqual(allGenericTypes))) continue;
                genericTypeUsedGenericTypes[genericTypeFound].Add(allGenericTypes);
            }
        }

// now we have a list of all generic types and their generic args
// iterate through each generic type and go through the static field process again
        foreach (var typeUsedGenericTypes in genericTypeUsedGenericTypes)
        {
            var type = typeUsedGenericTypes.Key;
            var genericArgs = typeUsedGenericTypes.Value;

            foreach (var args in genericArgs)
            {
                var genericTypeDefinition = type.MakeGenericType(args.ToArray());
                var fields = AccessTools.GetDeclaredFields(genericTypeDefinition).Where(x =>
                    x.IsStatic && !x.IsLiteral).ToArray();

                if (fields.Length == 0 && genericTypeDefinition.TypeInitializer == null) continue;

                Trace.Write(
                    $"type: {genericTypeDefinition.FullName} has {fields.Length} static fields, and static constructor: {genericTypeDefinition.TypeInitializer != null}");
                _staticFields.AddRange(fields);
            }
        }
    }

    private static bool FindGenericType(IGenericInstance genericInstanceType)
    {
        foreach (var genericArg in genericInstanceType.GenericArguments)
        {
            switch (genericArg)
            {
                case GenericParameter:
                case GenericInstanceType innerGenericInstance when FindGenericType(innerGenericInstance):
                    return true;
            }
        }

        return false;
    }

    public void ResetStaticFields()
    {
        Tracker.StopStaticCtorExecution = true;
        ResetFields();
        Tracker.StopStaticCtorExecution = false;
        InvokeStaticCtors();
    }

    private void ResetFields()
    {
        _logger.LogDebug("setting static fields");
        foreach (var field in _staticFields)
        {
            Trace.Write(
                $"Setting static field: {field.DeclaringType?.FullName ?? "unknown_type"}.{field.Name} to null");
            field.SetValue(null, null);
        }

        GC.Collect();
        GC.WaitForPendingFinalizers();
    }

    private void InvokeStaticCtors()
    {
        _logger.LogDebug("calling static constructors");
        var staticCtorInvokeOrderListCount = Tracker.StaticCtorInvokeOrderList.Count;
        for (var i = 0; i < staticCtorInvokeOrderListCount; i++)
        {
            var staticCtorType = Tracker.StaticCtorInvokeOrderList[i];
            var cctor = staticCtorType.TypeInitializer;
            if (cctor == null) continue;
            Trace.Write(
                $"Calling static constructor for type: {cctor.DeclaringType?.FullName ?? "unknown_type"}");
            cctor.Invoke(null, default);
        }
    }
}