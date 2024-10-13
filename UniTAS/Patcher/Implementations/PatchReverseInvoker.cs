using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using Mono.Cecil;
using MonoMod.Utils;
using UniTAS.Patcher.Extensions;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.Logging;

namespace UniTAS.Patcher.Implementations;

// ReSharper disable once ClassNeverInstantiated.Global
[Singleton]
[ExcludeRegisterIfTesting]
public class PatchReverseInvoker(ILogger logger) : IPatchReverseInvoker
{
    public bool Invoking => _depth > 0;
    private uint _depth;

    private uint _generatedCount;
    private const string GeneratedAssemblyPrefix = "UniTAS.Generated-";

    private readonly List<MethodDefinition> _workingMethods = new();
    private readonly List<AssemblyDefinition> _workingAssemblyDefs = new();

    public MethodBase RecursiveReversePatch(MethodBase original)
    {
        GenerateAssemblies(original);
        var asm = LoadGeneratedAssemblies();
        _generatedCount++;

        // find the method
        var methodType = original.GetRealDeclaringType() ?? throw new NullReferenceException();
        var originalParams = original.GetParameters();

        var reverseMethod = asm.GetType(methodType.SaneFullName())
            .GetMethods(AccessTools.all)
            .First(x =>
            {
                var methodParams = x.GetParameters();
                if (x.Name != original.Name || methodParams.Length != originalParams.Length) return false;
                return !methodParams.Where((t, i) =>
                    t.ParameterType.SaneFullName() != originalParams[i].ParameterType.SaneFullName()).Any();
            });

        foreach (var asmDef in _workingAssemblyDefs)
        {
            asmDef.Dispose();
        }

        _workingAssemblyDefs.Clear();
        _workingMethods.Clear();

        return reverseMethod;
    }

    private void GenerateAssemblies(MethodBase original)
    {
        // i dont care
        var type = original.GetRealDeclaringType() ?? throw new NullReferenceException();
        var typeAssembly = type.Assembly;

        var asmDef =
            AssemblyDefinition.ReadAssembly(Path.Combine(Paths.ManagedPath, $"{typeAssembly.GetName().Name}.dll"));
        asmDef.Name.Name = $"{GeneratedAssemblyPrefix}{_generatedCount}.Assembly-CSharp";
        var mainModule = asmDef.MainModule;
        var typeDef = mainModule.Types.First(x => x.FullName == type.SaneFullName());
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

        GenerateAssemblies(methodDef, mainModule);
    }

    private void GenerateAssemblies(MethodDefinition methodDef, ModuleDefinition mainModule)
    {
        var methodAsm = methodDef.Module.Assembly;
        if (!_workingAssemblyDefs.Contains(methodAsm))
        {
            _workingAssemblyDefs.Add(methodAsm);
        }

        if (_workingMethods.Contains(methodDef)) return;
        _workingMethods.Add(methodDef);

        if (!methodDef.HasBody) return;

        foreach (var instruction in methodDef.Body.Instructions)
        {
            switch (instruction.Operand)
            {
                case MethodReference methodRef:
                {
                    GenerateAssemblies(methodRef.Resolve(), mainModule);
                    break;
                }
                case FieldReference fieldRef:
                {
                    // find actual field
                    var fieldType = AccessTools.TypeByName(fieldRef.DeclaringType.FullName);
                    var field = fieldType.GetField(fieldRef.Name, AccessTools.all);
                    var newFieldRef = mainModule.ImportReference(field);
                    instruction.Operand = newFieldRef;
                    break;
                }
            }
        }
    }

    private Assembly LoadGeneratedAssemblies()
    {
        Assembly ret = null;
        for (var i = _workingAssemblyDefs.Count - 1; i > -1; i--)
        {
            var def = _workingAssemblyDefs[i];
            using var stream = new MemoryStream();
            def.Write(stream);
            ret = Assembly.Load(stream.GetBuffer());
        }

        return ret!;
    }

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