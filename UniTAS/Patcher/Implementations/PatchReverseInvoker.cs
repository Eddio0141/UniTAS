﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using BepInEx;
using HarmonyLib;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using MonoMod.Utils;
using UniTAS.Patcher.Extensions;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.Logging;
using TypeAttributes = Mono.Cecil.TypeAttributes;

namespace UniTAS.Patcher.Implementations;

// ReSharper disable once ClassNeverInstantiated.Global
[Singleton]
public class PatchReverseInvoker(ILogger logger) : IPatchReverseInvoker
{
    public bool Invoking => _depth > 0;
    private uint _depth;

    private readonly Dictionary<Assembly, AssemblyDefinition> _assemblyCache = new();

    public MethodInfo RecursiveReversePatch(MethodInfo original)
    {
        var originalHash = original.GetHashCode();
        var module = ModuleDefinition.CreateModule($"UniTAS.ReversePatching-{originalHash}",
            new ModuleParameters
            {
                Kind = ModuleKind.Dll,
                ReflectionImporterProvider = MMReflectionImporter.ProviderNoDefault
            });

        module.Assembly.CustomAttributes.Add(
            new CustomAttribute(module.ImportReference(UnverifiableCodeAttr)));

        // i dont care
        var type = original.GetRealDeclaringType() ?? throw new NullReferenceException();
        var typeAssembly = type.Assembly;

        if (!_assemblyCache.TryGetValue(typeAssembly, out var asmDef))
        {
            var location = typeAssembly.Location;
            asmDef = AssemblyDefinition.ReadAssembly(location.IsNullOrWhiteSpace()
                ? Path.Combine(Paths.ManagedPath, $"{typeAssembly.GetName().Name}.dll")
                : location);
            _assemblyCache.Add(typeAssembly, asmDef);
        }

        var typeDef = asmDef.MainModule.GetAllTypes().First(x =>
        {
            Type resolved;
            try
            {
                resolved = x.ResolveReflection();
            }
            catch (Exception)
            {
                return false;
            }

            return resolved == type;
        });
        var originalParams = original.GetParameters();
        var methodDef = typeDef.Methods.First(x =>
        {
            if (x.Name != original.Name || x.Parameters.Count != originalParams.Length) return false;
            for (var i = 0; i < x.Parameters.Count; i++)
            {
                if (x.Parameters[i].ParameterType.FullName != originalParams[i].ParameterType.SaneFullName())
                    return false;
            }

            return true;
        });

        var newDef = RecursiveReversePatch(methodDef, module, null, new());

        var asm = ReflectionHelper.Load(module);

        // find the method
        var reverseMethod = asm.GetType(newDef.DeclaringType.FullName)
            .GetMethods(AccessTools.all)
            .First(x =>
            {
                var methodParams = x.GetParameters();
                if (x.Name != newDef.Name || methodParams.Length != originalParams.Length) return false;
                return !methodParams.Where((t, i) =>
                    t.ParameterType.SaneFullName() != originalParams[i].ParameterType.SaneFullName()).Any();
            });

        module.Dispose();

        return reverseMethod;
    }

    /// <summary>
    /// Recursively generates reverse patching methods
    /// </summary>
    /// <param name="original">Original method</param>
    /// <param name="module">Module to patch the copied methods into</param>
    /// <param name="randomTypeDef">A type definition that has a random name containing reverse methods that doesn't matter</param>
    /// <param name="doneMethods">A dictionary with original method as key, and patched ones as the value</param>
    /// <returns>Full name of type containing method, and name of method</returns>
    private MethodDefinition RecursiveReversePatch(MethodDefinition original, ModuleDefinition module,
        TypeDefinition randomTypeDef, Dictionary<MethodDefinition, MethodDefinition> doneMethods)
    {
        if (doneMethods.TryGetValue(original, out var alreadyPatched)) return alreadyPatched;

        TypeDefinition typeDef;
        if (original.HasBody)
        {
            if (randomTypeDef == null)
            {
                randomTypeDef = new TypeDefinition(
                    "",
                    $"UniTAS-ReversePatching-{original.Name.Replace('.', '_')}-{original.GetHashCode()}",
                    TypeAttributes.Public | TypeAttributes.Abstract |
                    TypeAttributes.Sealed |
                    TypeAttributes.Class
                )
                {
                    BaseType = module.TypeSystem.Object
                };

                module.Types.Add(randomTypeDef);
            }

            typeDef = randomTypeDef;
        }
        else
        {
            var originalDeclaringType = original.DeclaringType;
            var typeDefSearch = module.Types.FirstOrDefault(x => x.FullName == originalDeclaringType.FullName);

            if (typeDefSearch == null)
            {
                typeDef = new TypeDefinition(originalDeclaringType.Namespace, originalDeclaringType.Name,
                    TypeAttributes.Public |
                    TypeAttributes.Abstract |
                    TypeAttributes.Sealed |
                    TypeAttributes.Class)
                {
                    BaseType = module.TypeSystem.Object
                };
                module.Types.Add(typeDef);
            }
            else
            {
                typeDef = typeDefSearch;
            }
        }

        var newMethod = original.Clone();
        newMethod.IsPublic = true;
        doneMethods.Add(original, newMethod);
        newMethod.DeclaringType = typeDef;

        var retRef = module.ImportReference(newMethod.ReturnType);
        newMethod.ReturnType = retRef;

        // import references
        foreach (var customAttr in newMethod.CustomAttributes)
        {
            var newRef = module.ImportReference(customAttr.Constructor);
            customAttr.Constructor = newRef;
        }

        foreach (var param in newMethod.Parameters)
        {
            var newRef = module.ImportReference(param.ParameterType);
            param.ParameterType = newRef;
        }

        typeDef.Methods.Add(newMethod);

        if (!original.HasBody) return newMethod;

        foreach (var ex in newMethod.Body.ExceptionHandlers)
        {
            var newRef = module.ImportReference(ex.CatchType);
            ex.CatchType = newRef;
        }

        foreach (var varDef in newMethod.Body.Variables)
        {
            var newRef = module.ImportReference(varDef.VariableType);
            varDef.VariableType = newRef;
        }

        newMethod.Name = $"{original.Name.Replace('.', '_')}-{original.GetHashCode()}";

        for (var i = 0; i < newMethod.Body.Instructions.Count; i++)
        {
            var instruction = newMethod.Body.Instructions[i];
            var originalInstruction = original.Body.Instructions[i];

            switch (instruction.Operand)
            {
                case MethodReference:
                {
                    var originalMethodRef = (MethodReference)originalInstruction.Operand;
                    var reverseMethod =
                        RecursiveReversePatch(originalMethodRef.Resolve(), module, randomTypeDef, doneMethods);
                    var newRef = module.ImportReference(reverseMethod);
                    instruction.Operand = newRef;
                    break;
                }
                case FieldReference fieldRef:
                {
                    // find actual field
                    var fieldType =
                        AccessTools.TypeByName(((FieldReference)originalInstruction.Operand).DeclaringType.FullName);
                    var field = fieldType.GetField(fieldRef.Name, AccessTools.all);
                    var newRef = module.ImportReference(field);
                    instruction.Operand = newRef;
                    break;
                }
            }
        }

        return newMethod;
    }

    private static readonly ConstructorInfo UnverifiableCodeAttr =
        typeof(UnverifiableCodeAttribute).GetConstructor(ArrayEx.Empty<Type>())!;

    public bool InnerCall()
    {
        if (_depth > 0)
        {
            _depth++;
        }

        return _depth > 0;
    }

    public void Return()
    {
        switch (_depth)
        {
            case 1:
                logger.LogError($"Something went wrong returning from reverse invoking, {new StackTrace()}");
                return;
            case > 0:
                _depth--;
                break;
        }
    }

    public void Invoke(Action method)
    {
        StartInvoke();
        method.Invoke();
        FinishInvoke();
    }

    public TRet Invoke<TRet>(Func<TRet> method)
    {
        StartInvoke();
        var ret = method.Invoke();
        FinishInvoke();
        return ret;
    }

    public TRet Invoke<TRet, T>(Func<T, TRet> method, T arg1)
    {
        StartInvoke();
        var ret = method.Invoke(arg1);
        FinishInvoke();
        return ret;
    }

    public TRet Invoke<TRet, T1, T2>(Func<T1, T2, TRet> method, T1 arg1, T2 arg2)
    {
        StartInvoke();
        var ret = method.Invoke(arg1, arg2);
        FinishInvoke();
        return ret;
    }

    private void StartInvoke()
    {
        _depth = 1;
    }

    private void FinishInvoke()
    {
        if (_depth != 1)
        {
            logger.LogWarning($"Depth isn't matching 1 after reverse invoking, {new StackTrace()}");
        }

        _depth = 0;
    }
}