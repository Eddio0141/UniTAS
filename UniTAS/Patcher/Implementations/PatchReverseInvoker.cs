using System;
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

namespace UniTAS.Patcher.Implementations;

// ReSharper disable once ClassNeverInstantiated.Global
[Singleton]
public class PatchReverseInvoker(ILogger logger) : IPatchReverseInvoker
{
    public bool Invoking => _depth > 0;
    private uint _depth;

    public MethodInfo RecursiveReversePatch(MethodInfo original)
    {
        var originalHash = original.GetHashCode();

        // i dont care
        var type = original.GetRealDeclaringType() ?? throw new NullReferenceException();
        var typeAssembly = type.Assembly;
        string typeFullName = null;

        var location = typeAssembly.Location;
        var asmDef = AssemblyDefinition.ReadAssembly(location.IsNullOrWhiteSpace()
            ? Path.Combine(Paths.ManagedPath, $"{typeAssembly.GetName().Name}.dll")
            : location);

        var module = asmDef.MainModule;

        var typeDef = asmDef.MainModule.GetAllTypes().First(x =>
        {
            Type resolved;
            try
            {
                resolved = x.ResolveReflection();
            }
            catch (Exception)
            {
                typeFullName ??= type.SaneFullName();
                return MonoModTypeNameToReflection(x.FullName) == typeFullName;
            }

            return resolved == type;
        });

        var methodDef = typeDef.GetMethods().First(x =>
        {
            MethodBase resolved;
            try
            {
                resolved = x.ResolveReflection();
            }
            catch (Exception)
            {
                return false;
            }

            return resolved == original;
        });

        var originalParams = original.GetParameters();

        var newDef = RecursiveReversePatch(methodDef, module, new());

        module.Name = $"UniTAS.ReversePatching-{originalHash}";
        asmDef.CustomAttributes.Add(new CustomAttribute(module.ImportReference(UnverifiableCodeAttr)));

        var asm = ReflectionHelper.Load(module);

        // find the method
        var reverseMethod = asm.GetType(MonoModTypeNameToReflection(newDef.DeclaringType.FullName))
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
    /// <param name="methodDef">Original method</param>
    /// <param name="module">Module to patch the copied methods into</param>
    /// <param name="doneMethods">A dictionary with original method as key, and patched ones as the value</param>
    /// <returns>Full name of type containing method, and name of method</returns>
    private static MethodDefinition RecursiveReversePatch(MethodDefinition methodDef, ModuleDefinition module,
        List<MethodDefinition> doneMethods)
    {
        if (doneMethods.Contains(methodDef)) return methodDef;
        doneMethods.Add(methodDef);

        methodDef.IsPublic = true;

        if (!methodDef.HasBody) return methodDef;

        // TODO:
        // newMethod.Name = $"{original.Name.Replace('.', '_')}-{original.GetHashCode()}";

        foreach (var instruction in methodDef.Body.Instructions)
        {
            switch (instruction.Operand)
            {
                case MethodReference methodRef:
                {
                    // TODO: method in external assembly must be also generated
                    RecursiveReversePatch(methodRef.Resolve(), module, doneMethods);
                    break;
                }
                case FieldReference fieldRef:
                {
                    // find actual field
                    Type declaringType;
                    try
                    {
                        declaringType = fieldRef.DeclaringType.ResolveReflection();
                    }
                    catch (Exception)
                    {
                        // find by name
                        var name = MonoModTypeNameToReflection(fieldRef.DeclaringType.FullName);

                        declaringType = AccessTools.TypeByName(name);
                    }

                    var field = declaringType.GetField(fieldRef.Name, AccessTools.all);
                    var newRef = module.ImportReference(field);
                    instruction.Operand = newRef;
                    break;
                }
            }
        }

        return methodDef;
    }

    private static string MonoModTypeNameToReflection(string name)
    {
        name = name.Replace('/', '+');

        while (true)
        {
            var genericStart = name.IndexOf('<');
            if (genericStart < 0) break;
            var genericEnd = name.IndexOf('>', genericStart + 1);
            if (genericEnd < 0) break;
            name = name.Remove(genericStart, genericEnd - genericStart + 1);
        }

        return name;
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